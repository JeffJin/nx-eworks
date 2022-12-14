using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.ffmpeg_media_services;
using adworks.media_common;
using adworks.media_common.Configuration;
using adworks.media_common.Services;
using adworks.message_bus;
using adworks.message_common;
using adworks.networking;
using Eworks.AdworksMediaProcessor.Processors;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_processor
{
    public class EventLoggingProcessor : BaseProcessor, IEventLoggingProcessor
    {
        private readonly IMessageClient _messageClient;
        private readonly IDeviceService _deviceService;
        private readonly ITextService _textService;
        private string _instanceId;

        public EventLoggingProcessor( IConfiguration configuration, ILogger logger, ITextService textService,
            IMessageClient messageClient, IDeviceService deviceService): base(configuration, logger, messageClient)
        {
            _messageClient = messageClient;
            _deviceService = deviceService;
            _textService = textService;
            var instance = configuration.GetSection("Instance").Get<Instance>();
            _instanceId = instance.Id;
        }

        public void Run()
        {
            string[] bindingKeys = {$"#.{MessageTopics.PiReportStatus}"};
            var queueName = $"{_instanceId}-{QueueNames.PiEventProcessingQueue}";
            _logger.Information($"EventLogging Processor subscribes to Exchange '{MessageExchanges.ProcessorExchange}', Queue '{queueName}', bindingKeys '{bindingKeys[0]}'");
            _subscription = _messageClient.Subscribe(MessageExchanges.ProcessorExchange, queueName, bindingKeys, ProcessStatusReport);
        }

        private async Task ProcessStatusReport(Message m)
        {
            try
            {
                _logger.Information("Device heartbeat message received!", m.Body);
                var dto = SerializeHelper.Deserialize<DeviceStatusDto>(m.Body);
                //add statuses into log file
                dto.ReceivedOn = DateTimeOffset.UtcNow;
                _textService.LogDeviceStatus(dto);

                //add or update latest status record in the database
                var task = _deviceService.AddOrUpdateDeviceStatus(dto);
                _tasks.Add(task);
                await task;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to process device heartbeat message");
            }
        }
    }
}
