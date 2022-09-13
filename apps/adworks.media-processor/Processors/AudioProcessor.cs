using System;
using System.IO;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;
using adworks.media_common.Configuration;
using adworks.message_bus;
using adworks.message_common;
using adworks.networking;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_processor.Processors
{
    public class AudioProcessor: IProcessor
    {
       private readonly IFFMpegService _ffMpegService;
        private readonly IAudioService _audioService;
        private readonly IMessageClient _messageClient;
        private readonly IFileService _fileService;
        private readonly IFtpService _ftpService;
        private readonly ILogger _logger;
        private ISubscription _subscription;

        private string _audioFolder;
        private IConfiguration _configuration;
        private string _ftpStramingAddress;
        private string _ftpProcessorAddress;
        private string _instanceId;

        public AudioProcessor(IFFMpegService ffMpegService, IAudioService audioService,
             IConfiguration configuration, ILogger logger, IMessageClient messageClient,
            IFileService fileService, IFtpService ftpService)
        {
            _ffMpegService = ffMpegService;
            _audioService = audioService;
            _messageClient = messageClient;
            _fileService = fileService;
            _ftpService = ftpService;
            _logger = logger;
            _configuration = configuration;

            _audioFolder = this._configuration["Ftp:AudioFolder"];
            _ftpStramingAddress = this._configuration["Ftp:StreamingAddress"];
            _ftpProcessorAddress = this._configuration["Ftp:ProcessorAddress"];
            var instance = configuration.GetSection("Instance").Get<Instance>();
            _instanceId = instance.Id;
        }

        public void Run()
        {
            // Warning: socket connection is creating an extra dependency on socket server(web api server),
            // thus using RabbitMQ to send notification may be better
            // start video encoding job
            // video encoding and thumbnail creation finished, notify the clients

            string[] bindingKeys = { "Audio.#" };
            var queueName = $"{_instanceId}-{QueueNames.AudioProcessingQueue}";
            _logger.Information($"Audio Processor subscribes to Exchange '{MessageExchanges.ProcessorExchange}', Queue '{queueName}', bindingKeys '{bindingKeys[0]}'");
            _subscription = _messageClient.Subscribe(MessageExchanges.ProcessorExchange, queueName, bindingKeys, HandleMessage);
        }

        private async Task HandleMessage(Message m)
        {
            if (m.Topic == MessageTopics.AudioUploaded)
            {
                await HandleAudioUploadMessage(m);
            }
            else
            {
                _logger.Error("Invalid message topic in image processor", m);
            }
        }

        private async Task HandleAudioUploadMessage(Message m)
        {
            try
            {
                _logger.Information("Audio uploaded message received!", m.Body);
                var details = SerializeHelper.Deserialize<AudioDto>(m.Body);

                _messageClient.Publish(new Message(m.MessageIdentity)
                {
                    Topic = MessageTopics.AudioStartProcessing,
                    Body = SerializeHelper.Stringify(details)
                }, MessageExchanges.UserExchange, $"{m.MessageIdentity.ToString()}");

                Audio audio = await ProcessUploadedAudio(details, m.MessageIdentity);

                var audioDto = DtoHelper.Convert(audio);
                _messageClient.Publish(new Message(m.MessageIdentity)
                {
                    Topic = MessageTopics.AudioFinishProcessing,
                    Body = SerializeHelper.Stringify(audioDto)
                }, MessageExchanges.UserExchange, $"{m.MessageIdentity.ToString()}");

            }
            catch (Exception e)
            {
                _logger.Error("Failed to proces AudioUploaded message", e);
                _messageClient.Publish(new Message(m.MessageIdentity)
                {
                    Topic = MessageTopics.AudioFailedProcessing,
                    Body = SerializeHelper.Stringify(new
                    {
                        Origin = m.Body,
                        Error = e.Message
                    })
                }, MessageExchanges.UserExchange, $"{m.MessageIdentity.ToString()}");
            }
        }

        private async Task<Audio> ProcessUploadedAudio(AudioDto dto, MessageIdentity messageIdentity)
        {
            try
            {
                //encode video
                _logger.Information("started encoding the audio :: " + dto.RawFilePath);
                var encodedUrl = await _ffMpegService.EncodeAudio(dto.Id, dto.RawFilePath, messageIdentity);
                dto.EncodedFilePath = encodedUrl;
                _logger.Information("finish encoding the audio :: " + dto.RawFilePath);

                var mediaInfo = _ffMpegService.GetMediaInfo(encodedUrl);

                dto.UpdatedOn = DateTimeOffset.UtcNow;

                dto.Duration = mediaInfo.Duration.TotalSeconds;

                dto.CloudUrl = TransferAudioToCloud(dto, messageIdentity);

                //save video to database
                await _audioService.AddAudio(dto);
                return await _audioService.FindAudio(dto.Id);
            }
            catch (FailedToEncodeAudioException ex)
            {
                //try again or remove uploaded file
                _logger.Error(ex, "Failed to transfer audio file to ftp site", dto);
                _fileService.RemoveAudio(dto.Id);
                return null;
            }
            catch (FailedToTransferToFtpSiteException ex)
            {
                //auto retry ftp upload after a few seconds before aborting
                _logger.Error(ex, "Failed to upload audio file to ftp site", dto);
                return null;
            }
            catch (Exception ex)
            {
                //clean up any generated files or try again
                _logger.Error(ex, "Failed to process audio file", dto);
                DeleteAudioData(dto.Id);
                return null;
            }
        }

        private string TransferAudioToCloud(AudioDto dto, MessageIdentity messageIdentity)
        {
            try
            {
                _logger.Information("Start publishing audio to cloud");

                _ftpService.Upload(dto.EncodedFilePath, _audioFolder + "/" + dto.CreatedBy + "/" + dto.Id,  _ftpStramingAddress, messageIdentity);
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
                        Topic = string.Format(MessageTopics.TemplateTransferToFtpSiteComplete, "Audio"),
                        Body = SerializeHelper.Stringify(evt)
                    }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}");

                    _logger.Information("Finish publishing audio to cloud");

                }
                catch (InvalidOperationException ioe)
                {
                    _logger.Error("RabbitMQ message sending failed", ioe);
                }

                string httpUrl = GetAudioHttpUrl(dto.CreatedBy, dto.Id.ToString(), Path.GetFileName(dto.EncodedFilePath));
                return httpUrl;

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove audio data", dto.Id);
                throw new FailedToTransferToFtpSiteException("failed to upload audio to ftp site " + dto.Id, ex);

            }
        }

        private string GetAudioHttpUrl(string userId, string mediaId, string fileName)
        {
            return _configuration["Audio:HttpBaseUrl"] + userId + "/" + mediaId + "/" + fileName;
        }

        public bool DeleteAudioData(Guid id)
        {
            try
            {
                _fileService.RemoveAudio(id);
                _audioService.DeleteAudio(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to remove audio data", id);
                return false;
            }
        }

        public void Dispose()
        {
            _subscription.Unsubscribe();
        }
    }
}
