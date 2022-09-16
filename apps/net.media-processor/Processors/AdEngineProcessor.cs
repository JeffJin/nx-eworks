using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.media_common;
using adworks.message_bus;
using adworks.message_common;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;


namespace adworks.media_processor
{
    public class AdEngineProcessor : IAdEngineProcessor
    {
        private readonly IFFMpegService _mediaService;
        private readonly IVideoService _dataService;
        private readonly ISocketService _socketService;
        private readonly IMessageClient _messageClient;
        private readonly IAdSearchEngine _adDSearchEngine;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private ISubscription _subscription;

        public AdEngineProcessor(IFFMpegService mediaService, IVideoService dataService, ISocketService socketService,
            ILogger logger, IMessageClient messageClient, IAdSearchEngine adDSearchEngine, IConfiguration configuration)
        {
            _mediaService = mediaService;
            _dataService = dataService;
            _socketService = socketService;
            _messageClient = messageClient;
            _adDSearchEngine = adDSearchEngine;
            _configuration = configuration;
            _logger = logger;

        }
        public void Run()
        {
            // start video encoding job
            // video encoding and thumbnail creation finished, notify the clients
            string queueName = "AdSuggestionQueue";
            string[] bindingKeys = { };
            _logger.Information($"AdEngine Processor subscribes to Exchange '{MessageExchanges.ProcessorExchange}', Queue '{queueName}', bindingKeys '{bindingKeys[0]}'");
            _subscription = _messageClient.Subscribe(MessageExchanges.ProcessorExchange, queueName, bindingKeys , ProcessMessage);
        }

        private async Task ProcessMessage(Message message)
        {
            try
            {
                _logger.Information("ClientCheckedIn message received!", message.Body);
                var details = SerializeHelper.Deserialize<AdSearchInput>(message.Body);
                var video = await _adDSearchEngine.Search(details);
                _messageClient.Publish(new Message(message.MessageIdentity)
                {
                    Topic = MessageTopics.PiPlayNextVideo,
                    Body = SerializeHelper.Stringify(video)
                }, MessageExchanges.DeviceExchange, $"{message.MessageIdentity.ToString()}");
            }
            catch (Exception e)
            {
                _logger.Error("Failed to process ClientCheckedIn message", e);
            }
        }

        public void Dispose()
        {
            _subscription.Unsubscribe();
        }
    }

    public class AdSearchInput
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        //other user contexts
        public KeyValuePair<string, string> Profiles { get; set; }
    }
}
