using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.message_bus;
using adworks.message_common;
using NReco.VideoConverter;
using Microsoft.Extensions.Configuration;
using NReco.VideoInfo;
using RabbitMQ.Client;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.ffmpeg_media_services
{
    public class FFMpegMediaService : IFFMpegService
    {
        private readonly IMessageClient _messageClient;
        private IConfiguration _configuration;
        private string _convertedImageFolder;
        private string _convertedAudioFolder;
        private string _convertedVideoFolder;
        private string _ffMpegToolPath;
        private string _ffMpegExeName;
        private string _fontsLocation;

        private ILogger _logger;

        public FFMpegMediaService(IMessageClient messageClient, ILogger logger, IConfiguration configuration)
        {
            _messageClient = messageClient;
            _logger = logger;

            this._configuration = configuration;

            var baseFolder = this._configuration["BaseAssetFolder"];

            if (!string.IsNullOrWhiteSpace(baseFolder) && !baseFolder.StartsWith("/"))
            {
                var sampleBaseFolder  = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                baseFolder = Path.Combine(sampleBaseFolder, this._configuration["BaseAssetFolder"]);
            }
            this._convertedImageFolder = Path.Combine(baseFolder, this._configuration["Image:ConvertedFolder"]);
            this._convertedAudioFolder = Path.Combine(baseFolder, this._configuration["Audio:ConvertedFolder"]);
            this._convertedVideoFolder = Path.Combine(baseFolder, this._configuration["Video:ConvertedFolder"]);
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this._ffMpegToolPath = Path.Combine(path, this._configuration["FFMpeg:ToolPath"]);
            this._ffMpegExeName = this._configuration["FFMpeg:ExeName"];
            this._fontsLocation = Path.Combine(path, this._configuration["Video:Fonts"]);

            NReco.VideoConverter.License.SetLicenseKey(
                this._configuration["FFMpeg:LicenseOwner"],
                this._configuration["FFMpeg:LicenseKey"]
            );
        }

        private FFProbe GetFFProbe()
        {
            var ffProbe = new FFProbe();
            ffProbe.ToolPath = this._ffMpegToolPath;
            return ffProbe;
        }

        public MediaInfo GetMediaInfo(string path)
        {
            var probe = GetFFProbe();
            return probe.GetMediaInfo(path);
        }

        private FFMpegConverter GetConverter()
        {
            if (string.IsNullOrEmpty(this._ffMpegExeName))
            {
                throw new InvalidDataException("FFMpeg exe name is not specified");
            }
            if (!Directory.Exists(this._ffMpegToolPath))
            {
                throw new InvalidDataException("FFMpeg tool path is not specified");
            }
            var ffMpeg = new FFMpegConverter
            {
                FFMpegExeName = this._ffMpegExeName,
                FFMpegToolPath = this._ffMpegToolPath

            };

            return ffMpeg;
        }

        public async Task<string> EncodeAudio(Guid audioId, string assetPath, MessageIdentity msgIdentity = null)
        {
            IModel sendChannel = null;
            try
            {
                var ffMpeg = GetConverter();

                ffMpeg.FFMpegProcessPriority = ProcessPriorityClass.Normal;
                ffMpeg.ConvertProgress += (o, args) =>
                {
                    // PROGRESS EVENT
                    _logger.Debug(string.Format("TotalDuration - {0}, Processed - {1}", args.TotalDuration,
                        args.Processed));
                    _logger.Information("sending encoding progress message", assetPath, args.Processed.TotalSeconds);

                    if (msgIdentity != null)
                    {
                        if (sendChannel == null)
                        {
                            sendChannel = _messageClient.CreateChannel();
                        }

                        _messageClient.Publish(new Message(msgIdentity)
                        {
                            Topic = MessageTopics.AudioEncode,
                            Body = SerializeHelper.Stringify(new ProgressEvent
                            {
                                JobName = assetPath,
                                Progress = Convert.ToInt16(args.Processed.TotalSeconds * 100 /
                                                           args.TotalDuration.TotalSeconds)
                            })
                        }, MessageExchanges.UserExchange, $"{msgIdentity.ToString()}", sendChannel);
                    }
                };

                return await Task.Run(() =>
                {
                    //TODO handle duplicated file names
                    var fileName = Path.GetFileNameWithoutExtension(assetPath);
                    string folder = Path.Combine(this._convertedAudioFolder, audioId.ToString());
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    var convertedPath = Path.Combine(folder, fileName + ".mp3");
                    //ffmpeg -i input.wav -codec:a libmp3lame -qscale:a 2 output.mp3
                    var settings = new ConvertSettings();
                    settings.CustomOutputArgs = "-codec:a libmp3lame -qscale:a 2";
                    ffMpeg.ConvertMedia(assetPath, null, convertedPath, "mp3", settings);
                    //clean up channel
                    sendChannel?.Dispose();
                    return convertedPath;
                });
            }
            catch (Exception ex)
            {
                sendChannel?.Dispose();

                throw new FailedToEncodeAudioException(ex);
            }
        }

        /// <summary>
        /// convert video to mp4 format
        /// </summary>
        /// <param name="id">asset entity Id</param>
        /// <param name="assetPath">uploaded image file path</param>
        /// <param name="messageIdentity">message identification data for notification purpose</param>
        /// <returns></returns>
        /// <exception cref="FailToEncodeVideoException"></exception>
        public async Task<string> EncodeVideo(Guid id, string assetPath, MessageIdentity messageIdentity = null)
        {
            IModel sendChannel = null;
            try
            {
                var ffMpeg = GetConverter();
                _logger.Information($"FFmpeg converter initialized");

                ffMpeg.FFMpegProcessPriority = ProcessPriorityClass.Normal;
                ffMpeg.ConvertProgress += (o, args) =>
                {
                    // PROGRESS EVENT
                    _logger.Information($"TotalDuration - {args.TotalDuration}, Processed - {args.Processed}");
                    _logger.Information("sending encoding progress message", assetPath, args.Processed.TotalSeconds);
                    if (messageIdentity != null)
                    {
                        if (sendChannel == null)
                        {
                            sendChannel = _messageClient.CreateChannel();
                        }
                        _logger.Information($"Message client publishing message ${args.Processed} - ${args.TotalDuration}");

                        _messageClient.Publish(new Message(messageIdentity)
                        {
                            Topic = MessageTopics.VideoEncode,
                            Body = SerializeHelper.Stringify(new ProgressEvent
                            {
                                JobName = assetPath,
                                Progress = Convert.ToInt16(args.Processed.TotalSeconds * 100 /
                                                           args.TotalDuration.TotalSeconds)
                            })
                        }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}", sendChannel);
                    }
                };

                return await Task.Run(() =>
                {
                    //TODO handle duplicated file names
                    var convertedPath = GetConvertedVideoPath(id, assetPath);

                    ffMpeg.ConvertMedia(assetPath, null, convertedPath, Format.mp4, new ConvertSettings()
                    {
                        CustomOutputArgs = "-preset veryfast",
                    });
                    sendChannel?.Dispose();
                    //clean up channel
                    return convertedPath;
                });
            }
            catch (Exception ex)
            {
                sendChannel?.Dispose();
                throw new FailToEncodeVideoException(ex);
            }
        }

        public IList<string> CreateThumbnails(string inputFile, int numberOfThumbnails, double duration)
        {
            var list = new List<string>();
            try
            {
                for (int i = 0; i < numberOfThumbnails; i++)
                {
                    Random rnd = new Random();
                    float pos = rnd.Next(1, Convert.ToInt32(duration));
                    //TODO
                    var thumbnail = CreateThumbnail(inputFile, pos, numberOfThumbnails * 10);
                    list.Add(thumbnail);
                }
                if (list.Count == 0)
                {
                    throw new FailToCreateThumbnailsException(new Exception("No thumbnails are created"));
                }
            }
            catch (Exception ex)
            {
                throw new FailToCreateThumbnailsException(ex);
            }

            return list;
        }

        public string CreateThumbnail(string inputFile, float pos, int maxRetry)
        {
            string fileName = Path.GetFileNameWithoutExtension(inputFile);
            string folderPath = Path.GetDirectoryName(inputFile);

            string thumbnailFolder = Path.Combine(folderPath, _configuration["Video:ThumbnailFolderName"]);

            if (!Directory.Exists(thumbnailFolder))
            {
                Directory.CreateDirectory(thumbnailFolder);
            }

            int count = 0;
            string outputFile = Path.Combine(thumbnailFolder, fileName + "_" + count + ".jpg");
            while (count < maxRetry)
            {
                if (File.Exists(outputFile))
                {
                    outputFile = Path.Combine(thumbnailFolder, fileName + "_" + count + ".jpg");
                }
                else
                {
                    break;
                }
                count++;
            }

            var ffMpegConverter = GetConverter();
            ffMpegConverter.GetVideoThumbnail(inputFile, outputFile, pos);
            return outputFile;
        }

        /// <summary>
        /// Add dynamic text to video at the bottom of the video
        /// </summary>
        /// <param name="videoId">video entity id</param>
        /// <param name="text">text content to display</param>
        /// <param name="filePath">uploaded video file path</param>
        /// <returns></returns>
        public string AddTextToVideo(Guid videoId, string text, string filePath, string color="blue", int fontSize = 25)
        {
            try
            {
                var ffMpeg = GetConverter();

                ffMpeg.FFMpegProcessPriority = ProcessPriorityClass.Normal;

                var convertedPath = GetConvertedVideoPath(videoId, filePath);

                var settings = new ConvertSettings();
                settings.CustomOutputArgs = string.Format(" -vf drawtext=\"fontfile={0}:text='{1}':x=w-t*50:y=h-th:fontcolor={2}:fontsize={3}\" -t 10",
                    _fontsLocation, text, color, fontSize);
                ffMpeg.ConvertMedia(filePath, null, convertedPath, Format.mp4, settings);

                //clean up channel
                return convertedPath;
            }
            catch (Exception ex)
            {
                throw new FailToAddTextToVideoException(ex);
            }
        }

        private string GetConvertedImagePath(Guid entityId, string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            string folder = Path.Combine(this._convertedImageFolder, entityId.ToString());
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, fileName + "." + Format.mp4);
        }

        private string GetConvertedVideoPath(Guid entityId, string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            string folder = Path.Combine(this._convertedVideoFolder, entityId.ToString());
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, fileName + "." + Format.mp4);
        }

        /// <summary>
        /// Convert image to video file with specified duration and add audio to it
        /// </summary>
        /// <param name="imageId">image entity Id</param>
        /// <param name="imagePath">uploaded image file path</param>
        /// <param name="audioPath">uploaded audio file path</param>
        /// <param name="duration">duration of the video to be generated</param>
        /// <returns>converted file location</returns>
        public string AddAudioToImage(Guid imageId, string imagePath, string audioPath, int duration)
        {
            try
            {
                var ffMpeg = GetConverter();

                ffMpeg.FFMpegProcessPriority = ProcessPriorityClass.Normal;

                var convertedPath = GetConvertedImagePath(imageId, imagePath);

                var settings = new ConvertSettings();
                settings.CustomInputArgs = string.Format("-loop 1 -i {0}", imagePath);
                settings.CustomOutputArgs = string.Format("-acodec aac -vcodec mpeg4 -t {0}", duration);
                ffMpeg.ConvertMedia(audioPath, null, convertedPath, Format.mp4, settings);

                //clean up channel
                return convertedPath;
            }
            catch (Exception ex)
            {
                throw new FailToAddTextToVideoException(ex);
            }
        }
    }
}
