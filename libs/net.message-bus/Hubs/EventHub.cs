using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.media_common.Extensions;
using adworks.message_common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using ILogger = Serilog.ILogger;
using SocketMessage = adworks.message_common.SocketMessage;

namespace adworks.message_bus
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EventHub: Hub, IEventHub, IEventHubConnections
    {
        private static readonly ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();
        private readonly ILogger _logger;

        private readonly IDictionary<string, Func<string, Task>> _handlers =
            new Dictionary<string, Func<string, Task>>();

        public EventHub(ILogger logger)
        {
            _logger = logger;
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            string key = "*";

            Task handler(string message)
            {
                _logger.Information($"RegisterHandlers for {key}, {message}");
                return Task.CompletedTask;
            }

            this._handlers.Add(key, handler);
        }

        public async Task SendToCurrent(string message)
        {
            if (Context?.User?.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                var user = Context.User;

                foreach (var connectionId in _connections.GetConnections(user.GetId()))
                {
                    var msg = new SocketMessage()
                    {
                        Identity = new MessageIdentity(user.GetOrganization(), null, user.GetId()),
                        Message = message
                    };
                    await Clients.Client(connectionId).SendAsync("SendToCurrent", SerializeHelper.Stringify(msg));
                }
            }
        }

        public Task Receive(string clientId, string message)
        {
            if (Context?.User?.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                //TODO: deserialize the message and find corresponding handler to handle the message
                var connection = _connections.GetConnections(clientId).FirstOrDefault();
                _logger.Information($"Received message from {clientId}, {message}");
                var payload = SerializeHelper.Deserialize<SocketMessage>(message);
                if (_handlers.ContainsKey(payload.Identity.Identity))
                {
                    return _handlers[payload.Identity.Identity].Invoke(payload.Message);
                }
            }

            return Task.CompletedTask;
        }

        public async Task Send(string clientId, string message)
        {
            if (Context?.User?.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                foreach (var connectionId in _connections.GetConnections(clientId))
                {
                    var msg = new SocketMessage()
                    {
                        Identity = new MessageIdentity(null, null, clientId),
                        Message = message
                    };
                    // "Send" function name to be called on client side
                    await Clients.Client(connectionId).SendAsync("Send", SerializeHelper.Stringify(msg));
                }
            }
        }

        public async Task Broadcast(string message)
        {
            if (Context?.User?.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                var user = Context.User;

                var msg = new SocketMessage()
                {
                    Identity = new MessageIdentity(user.GetOrganization(), null, user.GetId()),
                    Message = message
                };
                await Clients.All.SendAsync("Broadcast", SerializeHelper.Stringify(msg));
            }
        }

        public async Task SendToGroup(string group, string message)
        {
            if (Context != null && Context.User != null &&  Context.User.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                var user = Context.User;

                var msg = new SocketMessage()
                {
                    Identity = new MessageIdentity(user.GetOrganization(), null, user.GetId()),
                    Message = message
                };
                await Clients.Group(group).SendAsync("SendToGroup", SerializeHelper.Stringify(msg));
            }
        }

        public override async Task OnConnectedAsync()
        {
            if (Context != null && Context.User != null && Context.User.Identity != null &&
                Context.User.Identity.IsAuthenticated)
            {
                var user = Context.User;

                if (!_connections.GetConnections(user.GetId()).Contains(Context.ConnectionId))
                {
                    _connections.Add(user.GetId(), Context.ConnectionId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            if (Context != null && Context.User != null && Context.User.Identity != null &&
                Context.User.Identity.IsAuthenticated)
            {
                var user = Context.User;
                _connections.Remove(user.GetId(), Context.ConnectionId);
                await base.OnDisconnectedAsync(ex);
            }

        }

        public IEnumerable<string> GetConnections(string userId)
        {
            return _connections.GetConnections(userId);
        }
    }
}
