using System.Threading;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.message_bus;
using adworks.message_common;
using adworks.pi_player.Scheduling;
using adworks.pi_player.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Processors
{
    public class AuditProcessor : IAuditProcessor
    {
        private readonly IMessageClient _messageClient;
        private static ILogger _logger;

        public AuditProcessor(ILogger logger, IMessageClient messageClient)
        {
            _logger = logger;
            _messageClient = messageClient;
        }

        public async Task<ISubscription> Run()
        {
            _logger.Information("Audit processor running on thread {threadId}",
                Thread.CurrentThread.ManagedThreadId);
            string[] keys = {"#"};
            await Task.CompletedTask;
            return _messageClient.Subscribe(MessageExchanges.DeviceExchange, "PiEventsAuditingQuque", keys, AditMessage);
        }

        private Task AditMessage(Message message)
        {
            _logger.Information("Auditing message {topic}, {identity}", message.Topic, message.MessageIdentity);

            return Task.CompletedTask;
        }
    }
}
