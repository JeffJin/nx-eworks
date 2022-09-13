using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;
using adworks.media_common.Configuration;
using adworks.message_bus;
using adworks.message_common;
using adworks.networking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_processor
{
    public class VideoProcessor : IProcessor
    {
        private readonly IFFMpegService _ffMpegService;
        private readonly IVideoService _videoService;
        private readonly IMessageClient _messageClient;
        private readonly IFileService _fileService;
        private readonly IFtpService _ftpService;
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        private string _convertedVideoFolder;
        private string _baseAssetFolder;
        private string _videoFolder;
        private string _ftpStramingAddress;
        private string _ftpProcessorAddress;
        private IConfiguration _configuration;
        private ISubscription _subscription;
        private string _instanceId;

        public VideoProcessor(IFFMpegService ffMpegService, IVideoService videoService,
            IConfiguration configuration, IMessageClient messageClient, IFileService fileService, IFtpService ftpService,
            IDataContextFactory dbContextFactory, ILogger logger)
        {
            _ffMpegService = ffMpegService;
            _videoService = videoService;
            _messageClient = messageClient;
            _fileService = fileService;
            _ftpService = ftpService;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _configuration = configuration;

            _baseAssetFolder = _configuration["BaseAssetFolder"];
            _convertedVideoFolder = Path.Combine(_baseAssetFolder, _configuration["Video:ConvertedFolder"]);
            _videoFolder = this._configuration["Ftp:VideoFolder"];
            _ftpStramingAddress = this._configuration["Ftp:StreamingAddress"];
            _ftpProcessorAddress = this._configuration["Ftp:ProcessorAddress"];
            var instance = configuration.GetSection("Instance").Get<Instance>();
            _instanceId = instance.Id;
        }

        public void Run()
        {
            // Warning: socket connection is creating an extra dependency on socket server(web api server),
            // thus using RabbitMQ to send notification may be better
            string[] bindingKeys =
            {
                "Video.#"
            };
            var queueName = $"{_instanceId}-{QueueNames.VideoProcessingQueue}";
            _logger.Information($"Video Processor subscribes to Exchange '{MessageExchanges.ProcessorExchange}', Queue '{queueName}', bindingKeys '{bindingKeys[0]}'");
            _subscription = _messageClient.Subscribe(MessageExchanges.ProcessorExchange, queueName, bindingKeys, HandleMessage);
        }

        private async Task HandleMessage(Message m)
        {
            if (m.Topic == MessageTopics.VideoUploaded)
            {
                await HandleVideoUploadMessage(m);
            }
            else if (m.Topic == MessageTopics.VideoAddText)
            {
                await HandleVideoAddTextMessage(m);
            }
            else
            {
                _logger.Error("Invalid message topic in video processor", m);
            }
        }

        private async Task HandleVideoAddTextMessage(Message m)
        {
            var dto = SerializeHelper.Deserialize<VideoAddText>(m.Body);
            var result = await AddText(dto);

            var message = new Message(m.MessageIdentity)
            {
                Topic = MessageTopics.VideoAddTextResult,
                Body = SerializeHelper.Stringify(result)
            };

            _messageClient.Publish(message, MessageExchanges.UserExchange, $"{m.ToString()}");
        }

        private async Task HandleVideoUploadMessage(Message m)
        {
            try
            {
                _logger.Information("Video uploaded message received!", m.Body);
                var details = SerializeHelper.Deserialize<VideoDto>(m.Body);

                _messageClient.Publish(new Message(m.MessageIdentity)
                {
                    Topic = MessageTopics.VideoStartProcessing,
                    Body = SerializeHelper.Stringify(details)
                }, MessageExchanges.UserExchange, $"{m.MessageIdentity.ToString()}");

                var video = await ProcessUploadedVideo(details, m.MessageIdentity);

                var videoDto = DtoHelper.Convert(video);
                _messageClient.Publish(new Message(m.MessageIdentity)
                {
                    Topic = MessageTopics.VideoFinishProcessing,
                    Body = SerializeHelper.Stringify(videoDto)
                }, MessageExchanges.UserExchange, $"{m.MessageIdentity.ToString()}");
            }
            catch (Exception e)
            {
                _logger.Error("Failed to process VideoUploaded message", e);
                _messageClient.Publish(new Message(m.MessageIdentity)
                {
                    Topic = MessageTopics.VideoFailedProcessing,
                    Body = SerializeHelper.Stringify(new
                    {
                        Origin = m.Body,
                        Error = e.Message
                    })
                }, MessageExchanges.UserExchange, $"{m.MessageIdentity.ToString()}");
            }
        }

        private async Task<string> EncodeFile(VideoDto videoData, MessageIdentity messageIdentity)
        {
            //download cloud url to local for processing
            var localPath = _ftpService.Download(videoData.CloudUrl);
            var encodedUrl =
                await _ffMpegService.EncodeVideo(videoData.Id, localPath, messageIdentity);
            videoData.EncodedFilePath = encodedUrl;
            _logger.Information($"finish encoding the video :: {localPath} :: {encodedUrl}");
            _fileService.DeleteLocalFile(localPath);
            _logger.Information($"remove temporary video file :: {localPath}");
            return encodedUrl;
        }

        //encode video and create thumbnails
        //TODO the following block of logic should be in a transaction scope
        public async Task<Video> ProcessUploadedVideo(VideoDto videoData, MessageIdentity messageIdentity)
        {
            Exception exception = null;
            try
            {
                //encode video
                _logger.Information("started encoding the video :: " + videoData.CloudUrl);

                string encodedUrl = await EncodeFile(videoData, messageIdentity);

                //create thumbnail
                _logger.Information("started creating thumbnails for the video :: " + encodedUrl);
                var videoInfo = _ffMpegService.GetMediaInfo(encodedUrl);

                var thumbnails = _ffMpegService.CreateThumbnails(encodedUrl,
                    Convert.ToInt16(_configuration["Video:NumberOfThumbnails"]), videoInfo.Duration.TotalSeconds);

                videoData.Thumbnails = GetThumbnailUrls(thumbnails, videoData.CreatedBy).ToList();
                videoData.MainThumbnail = videoData.Thumbnails[0];
                videoData.UpdatedOn = DateTimeOffset.UtcNow;

                videoData.Duration = videoInfo.Duration.TotalSeconds;
                _logger.Information("finish creating thumbnails for the video :: " + encodedUrl);

                var urls = TransferVideoToCloud(videoData, messageIdentity);
                videoData.ProgressiveUrl = urls.ProgressiveUrl;
                videoData.CloudUrl = urls.VodVideoUrl;
                _logger.Information($"{videoData.Title} is uploaded to server at {videoData.CloudUrl}");

                //save video to database
                _logger.Information("start persisting the video metadata into mysql :: " + videoData.CloudUrl);
                await _videoService.AddOrUpdateVideo(videoData);
                _logger.Information("finish persisting the video metadata into mysql :: " + videoData.CloudUrl);
                return await _videoService.FindVideo(videoData.Id);
            }
            catch (FailToEncodeVideoException ex)
            {
                //try again or remove uploaded file
                _logger.Error(ex, "Failed to encode video file", videoData);
                _fileService.RemoveVideo(videoData.Id);
                exception = ex;
            }
            catch (FailToCreateThumbnailsException ex)
            {
                //try again or remove converted file and uploaded file
                _logger.Error(ex, "Failed to create thumbnail for video", videoData);
                _fileService.RemoveVideo(videoData.Id);
                exception = ex;
            }
            catch (FailedToTransferToFtpSiteException ex)
            {
                //auto retry ftp upload after a few seconds before aborting
                _logger.Error(ex, "Failed to upload video file to ftp site", videoData);
                exception = ex;
            }
            catch (Exception ex)
            {
                //clean up any generated files or try again
                _logger.Error(ex, "Failed to process video file", videoData);
                exception = ex;
            }
            finally
            {
                try
                {
                    if (exception != null)
                    {
                        DeleteVideoData(videoData.Id);
                        var bodyDto = new
                        {
                            Type = "Error",
                            Message = exception.Message
                        };
                        _messageClient.Publish(new Message(messageIdentity)
                        {
                            Topic = MessageTopics.VideoFailedProcessing,
                            Body = SerializeHelper.Stringify(bodyDto)
                        }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to clean up video file", videoData);
                }

            }
            return null;
        }

        private UrlResult TransferVideoToCloud(VideoDto video, MessageIdentity messageIdentity)
        {
            try
            {
                _logger.Information("Start publishing video to cloud");

                string folderPath = Path.GetDirectoryName(video.EncodedFilePath);
                //TODO Fix ImageQueue message sending issue when thumbnail files are uploaded
                _ftpService.UploadFolder(folderPath, _videoFolder + "/" + video.CreatedBy + "/" + video.Id,  _ftpStramingAddress, messageIdentity);
                try
                {
                    var evt = new SystemEvent()
                    {
                        Source = EventSources.MessageProcessor,
                        SourceId = video.Id,
                        MessageTopic = video.ProgressiveUrl
                    };

                    _messageClient.Publish(new Message(messageIdentity)
                    {
                        Topic = string.Format(MessageTopics.TemplateTransferToFtpSiteComplete, "Video"),
                        Body = SerializeHelper.Stringify(evt)
                    }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}");

                    _logger.Information("Finish publishing video to cloud");

                }
                catch (InvalidOperationException ioe)
                {
                    _logger.Error("RabbitMQ message sending failed", ioe);
                }

                string progressiveUrl = GetRtmpUrl(video.CreatedBy, video.Id.ToString(), Path.GetFileName(video.EncodedFilePath));
                string httpUrl = GetVideoHttpUrl(video.CreatedBy, video.Id.ToString(), Path.GetFileName(video.EncodedFilePath));
                return new UrlResult()
                {
                    ProgressiveUrl = progressiveUrl,
                    VodVideoUrl = httpUrl
                };

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove video data", video.Id);
                throw new FailedToTransferToFtpSiteException("failed to upload video to ftp site " + video.Id, ex);

            }
        }

        public bool DeleteVideoData(Guid videoId)
        {
            try
            {
                _fileService.RemoveVideo(videoId);
                _videoService.DeleteVideo(videoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove video data", videoId);
                return false;
            }
        }

        private IEnumerable<string> GetThumbnailUrls(IList<string> links, string userId)
        {
            return from link in links
                select _configuration["Video:HttpBaseUrl"] + userId + "/" + link.Replace(Path.Combine(_baseAssetFolder, _convertedVideoFolder), "");
        }

        private string GetRtmpUrl(string userId, string videoId, string fileName)
        {
            return _configuration["Video:RtmpBaseUrl"] + userId + "/" + videoId + "/" + fileName;
        }


        private string GetVideoHttpUrl(string userId, string mediaId, string fileName)
        {
            return _configuration["Video:HttpBaseUrl"] + userId + "/" + mediaId + "/" + fileName;
        }


        private async Task<Video> AddText(VideoAddText dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var model = await dbContext.Videos.Include(v => v.Category)
                    .SingleOrDefaultAsync(v => v.Id == dto.VideoId);
                if (model == null)
                {
                    throw new VideoNotFoundException(dto.VideoId);
                }


                string result = _ffMpegService.AddTextToVideo(dto.VideoId, dto.Text, model.RawFilePath,
                    dto.Color, dto.FontSize);

                model.EncodedVideoPath = result;
                var user = await dbContext.Users.Where(u => u.Email == dto.CreatedBy).SingleOrDefaultAsync();
                if (user != null)
                {
                    model.UpdatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(dto.CreatedBy);
                }

                await dbContext.SaveChangesAsync();

                return model;
            }

        }

        public void Dispose()
        {
            _subscription.Unsubscribe();
        }
    }
}
