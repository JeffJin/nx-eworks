using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.media_common.Extensions;
using adworks.message_common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.message_bus
{
    public class UserQueueMessageHandler : IUserQueueMessageHandler
    {
        private readonly IHubContext<EventHub> _hubContext;
        private readonly IEventHubConnections _hubConnections;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger _logger;

        public UserQueueMessageHandler(IHubContext<EventHub> hubContext, IEventHubConnections hubConnections,
            IHttpContextAccessor contextAccessor, ILogger logger)
        {
            _hubContext = hubContext;
            _hubConnections = hubConnections;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        private bool ValidateSecurityContext(MessageIdentity messageIdentity)
        {
            var jwtSecurityToken = SecurityUtils.GetJwtToken(_contextAccessor.HttpContext);
            string organization = jwtSecurityToken.GetOrganization();
            string userName = jwtSecurityToken.GetId();
            string sessionId = jwtSecurityToken.GetJti();
            return messageIdentity.Organization == organization && messageIdentity.Group == userName &&
                   messageIdentity.Identity == sessionId;
        }

        public async Task Handle(Message message)
        {
            //check security context
            var isValid = ValidateSecurityContext(message.MessageIdentity);

            //find SignalR client by user email(source)
            var connections = _hubConnections.GetConnections(message.MessageIdentity.Identity).ToList();

            if (message.Topic.StartsWith("Image"))
            {
                _logger.Information($"Process Image event, {message}");
                await HandleImageMessage(message, connections);
            }
            else if (message.Topic.StartsWith("Audio"))
            {
                _logger.Information($"Process Audio event, {message}");
                await HandleAudioMessage(message, connections);
            }
            else if (message.Topic.StartsWith("Video"))
            {
                _logger.Information($"Process Video event, {message}");
                await HandleVideoMessage(message, connections);
            }
            else if (message.Topic.StartsWith("Pi"))
            {
                _logger.Information($"Process PI event, {message}");
                await HandlePiEventMessage(message, connections);
            }
            else if (message.Topic.StartsWith("FTP"))
            {
                _logger.Information($"Process FTP event, {message}");
                await HandleFTPEventMessage(message, connections);
            }
            else
            {
                _logger.Information($"Process generic event, {message}");
                await SendToClient(message.Topic, message, connections);
            }
        }

        private async Task HandleVideoMessage(Message message, List<string> connections)
        {
            await SendToClient("VideoEvent", message, connections);
        }

        private async Task HandleAudioMessage(Message message, List<string> connections)
        {
            await SendToClient("AudioEvent", message, connections);
        }

        private async Task HandleImageMessage(Message message, List<string> connections)
        {
            await SendToClient("ImageEvent", message, connections);
        }

        private async Task HandlePiEventMessage(Message message, List<string> connections)
        {
            await SendToClient("PiEvent", message, connections);
        }

        private async Task HandleFTPEventMessage(Message message, List<string> connections)
        {
            await SendToClient("FTPEvent", message, connections);
        }

        private async Task SendToClient(string action, Message message, List<string> connections)
        {
            //Check security context

            foreach (var connectionId in connections)
            {
                var msg = new SocketMessage()
                {
                    Identity = message.MessageIdentity,
                    Message = $"{SerializeHelper.Stringify(message)}"
                };
                await _hubContext.Clients.Client(connectionId).SendAsync(action, SerializeHelper.Stringify(msg));
            }
        }
    }
}
