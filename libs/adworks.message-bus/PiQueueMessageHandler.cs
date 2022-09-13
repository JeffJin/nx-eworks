using System.Collections.Generic;
using System.Linq;
using adworks.media_common;
using adworks.message_common;
using Microsoft.AspNetCore.SignalR;

namespace adworks.message_bus
{
    public class PiQueueMessageHandler : IPiQueueMessageHandler
    {
        private readonly IHubContext<EventHub> _hubContext;
        private readonly IEventHubConnections _hubConnections;

        public PiQueueMessageHandler(IHubContext<EventHub> hubContext, IEventHubConnections hubConnections)
        {
            _hubContext = hubContext;
            _hubConnections = hubConnections;
        }

        public void Handle(Message message)
        {
            //Note: for pi device sent messages, device serial number will be the source and they are not currently registered as signalR client
            IList<string> userNames = GetUsersForDevice(message.MessageIdentity.Identity);
            IList<string> connections = new List<string>();
            foreach (var userName in userNames)
            {
                IList<string> connectionIds = _hubConnections.GetConnections(userName).ToList();
                foreach (var connectionId in connectionIds)
                {
                    connections.Add(connectionId);
                }
            }
            
            foreach (var connectionId in connections)
            {
                var msg = new SocketMessage()
                {
                    Identity = message.MessageIdentity,
                    Message = $"{SerializeHelper.Stringify(message)}"
                };
                _hubContext.Clients.Client(connectionId).SendAsync("PiEvent", SerializeHelper.Stringify(msg));
            }
        }

        private IList<string> GetUsersForDevice(string serialNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}