using System;
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
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_processor.Processors
{
    public class ImageProcessor: IProcessor
    {
       private readonly IFFMpegService _ffMpegService;
        private readonly IImageService _imageService;
        private readonly IMessageClient _messageClient;
        private readonly IFileService _fileService;
        private readonly IFtpService _ftpService;
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        private string _imageFolder;
        private IConfiguration _configuration;
        private ISubscription _subscription;
        private string _ftpStreamingAddress;
        private string _instanceId;

        public ImageProcessor(IFFMpegService ffMpegService, IImageService imageService,  IConfiguration configuration, ILogger logger,
            IMessageClient messageClient, IFileService fileService, IFtpService ftpService, IDataContextFactory dbContextFactory)
        {
            _ffMpegService = ffMpegService;
            _imageService = imageService;
            _messageClient = messageClient;
            _fileService = fileService;
            _ftpService = ftpService;
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _configuration = configuration;

            _imageFolder = this._configuration["Ftp:ImageFolder"];
            _ftpStreamingAddress = this._configuration["Ftp:StreamingAddress"];
            var instance = configuration.GetSection("Instance").Get<Instance>();
            _instanceId = instance.Id;
        }

        public void Run()
        {
            string[] bindingKeys =
            {
                "Image.#"
            };
            var queueName = $"{_instanceId}-{QueueNames.ImageProcessingQueue}";
            _logger.Information($"Image Processor subscribes to Exchange '{MessageExchanges.ProcessorExchange}', Queue '{queueName}', bindingKeys '{bindingKeys[0]}'");
            _subscription = _messageClient.Subscribe(MessageExchanges.ProcessorExchange, queueName, bindingKeys, HandleMessage);
        }

        private async Task HandleMessage(Message m)
        {
            if (m.Topic == MessageTopics.ImageUploaded)
            {
                await HandleImageUploadMessage(m);
            }
            else if (m.Topic == MessageTopics.ImageMergeAudio)
            {
                await HandleImageMergeAudioMessage(m);
            }
            else
            {
                _logger.Error("Invalid message topic in image processor", m);
            }
        }

        private async Task HandleImageMergeAudioMessage(Message m)
        {
            _logger.Information("Image merge audio message received!", m.Body);
            var dto = SerializeHelper.Deserialize<MergeAudioDto>(m.Body);
            var result = await Merge(dto.ImageId, dto.AudioId, dto.Duration, dto.User);

            var message = new Message(m.MessageIdentity)
            {
                Topic = MessageTopics.ImageMergeAudioFinished,
                Body = SerializeHelper.Stringify(result)
            };

            _messageClient.Publish(message, MessageExchanges.UserExchange, $"{m.ToString()}");

        }

        private async Task HandleImageUploadMessage(Message m)
        {
            try
            {
                _logger.Information("Image uploaded message received!", m.Body);
                var details = SerializeHelper.Deserialize<ImageDto>(m.Body);

                _messageClient.Publish(new Message(m.MessageIdentity)
                {
                    Topic = MessageTopics.ImageStartProcessing,
                    Body = SerializeHelper.Stringify(details)
                }, MessageExchanges.UserExchange, $"{m.MessageIdentity.ToString()}");

                Image image = await ProcessUploadedImage(details, m.MessageIdentity);

                ImageDto imageDto = DtoHelper.Convert(image);
                _messageClient.Publish(new Message(m.MessageIdentity)
                {
                    Topic = MessageTopics.ImageFinishProcessing,
                    Body = SerializeHelper.Stringify(imageDto)
                }, MessageExchanges.UserExchange, $"{m.MessageIdentity.ToString()}");

            }
            catch (Exception e)
            {
                _logger.Error("Failed to process ImageUploaded message", e);
                _messageClient.Publish(new Message(m.MessageIdentity)
                {
                    Topic = MessageTopics.ImageFailedProcessing,
                    Body = SerializeHelper.Stringify(new
                    {
                        Origin = m.Body,
                        Error = e.Message
                    })
                }, MessageExchanges.UserExchange, $"{m.MessageIdentity.ToString()}");
            }
        }

        private async Task<Image> ProcessUploadedImage(ImageDto dto, MessageIdentity messageIdentity)
        {
            try
            {
                dto.UpdatedOn = DateTimeOffset.UtcNow;

                dto.CloudUrl = TransferImageToCloud(dto, messageIdentity);

                //save video to database
                var image = await _imageService.AddOrUpdateImage(dto);
                return await _imageService.FindImage(image.Id);
            }
            catch (FailedToEncodeAudioException ex)
            {
                //try again or remove uploaded file
                _logger.Error(ex, "Failed to transfer image file to ftp site", dto);
                _fileService.RemoveImage(dto.Id);
                return null;
            }
            catch (FailedToTransferToFtpSiteException ex)
            {
                //auto retry ftp upload after a few seconds before aborting
                _logger.Error(ex, "Failed to upload image file to ftp site", dto);
                return null;
            }
            catch (Exception ex)
            {
                //clean up any generated files or try again
                _logger.Error(ex, "Failed to process image file", dto);
                DeleteImageData(dto.Id);
                return null;
            }
        }

        private string TransferImageToCloud(ImageDto dto, MessageIdentity messageIdentity)
        {
            try
            {
                _logger.Information("Start publishing image to cloud");

                _ftpService.Upload(dto.RawFilePath, _imageFolder + "/" + dto.CreatedBy + "/" + dto.Id, _ftpStreamingAddress, messageIdentity);
                try
                {
                    var evt = new SystemEvent()
                    {
                        Source = EventSources.MessageProcessor,
                        SourceId = dto.Id,
                        MessageTopic = dto.CloudUrl
                    };

                    _messageClient.Publish(new Message(messageIdentity)
                    {
                        Topic = string.Format(MessageTopics.TemplateTransferToFtpSiteComplete, "Image"),
                        Body = SerializeHelper.Stringify(evt)
                    }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}");

                    _logger.Information("Finish publishing image to cloud");

                }
                catch (InvalidOperationException ioe)
                {
                    _logger.Error("RabbitMQ message sending failed", ioe);
                }

                string httpUrl = GetImageHttpUrl(dto.CreatedBy, dto.Id.ToString(), Path.GetFileName(dto.RawFilePath));
                return httpUrl;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove image data", dto.Id);
                throw new FailedToTransferToFtpSiteException("failed to upload image to ftp site " + dto.Id, ex);

            }
        }

        public bool DeleteImageData(Guid id)
        {
            try
            {
                _fileService.RemoveImage(id);
                _imageService.DeleteImage(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove image data", id);
                return false;
            }
        }

        private string GetImageHttpUrl(string userId, string mediaId, string fileName)
        {
            return _configuration["Image:HttpBaseUrl"] + userId + "/" + mediaId + "/" + fileName;
        }

        private async Task<Video> Merge(Guid imageId, Guid audioId, int duration, UserDto userDto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var image = await dbContext.Images.FindAsync(imageId);

                var audio = await dbContext.Audios.FindAsync(audioId);

                if (audio == null)
                {
                    throw new AudioNotFoundException(audioId);
                }
                if (image == null)
                {
                    throw new ImageNotFoundException(imageId);
                }

                string mergedUrl = _ffMpegService.AddAudioToImage(image.Id, image.RawFilePath,
                    audio.EncodedFilePath, duration);

                var video = new Video()
                {
                    Category = image.Category,
                    CreatedOn = DateTimeOffset.UtcNow,
                    Title = image.Title + " " + audio.Title,
                    Tags = image.Tags + "," + audio.Tags,
                    Description = image.Description + " " + audio.Description,
                    Duration = duration,
                    EncodedVideoPath = mergedUrl,
                    ProgressiveVideoUrl = ""
                };

                var mergeRecord = new MergeRecord()
                {
                    AssetId1 = imageId,
                    AssetId2 = audioId,
                    CreatedBy = null,//TODO
                    CreatedOn = DateTimeOffset.UtcNow,
                    MergeType = MergeTypes.ImageAudio
                };
                var user = await dbContext.Users.Where(u => u.Email == userDto.Email).SingleOrDefaultAsync();
                if (user != null)
                {
                    video.CreatedBy = user;
                    mergeRecord.CreatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(userDto.Email);
                }
                await dbContext.MergeRecords.AddAsync(mergeRecord);
                await dbContext.Videos.AddAsync(video);
                await dbContext.SaveChangesAsync();
                return video;
            }
        }

        public void Dispose()
        {
            _subscription.Unsubscribe();
        }
    }
}
