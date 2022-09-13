using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.media_common.Configuration;
using adworks.media_common.Extensions;
using adworks.message_common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.message_bus
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RabbitUserSubscriptionService: IRabbitListener
    {
        private readonly IMessageClient _messageClient;
        private readonly IUserQueueMessageHandler _userQueueMessageHandler;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ConcurrentDictionary<string, ISubscription> _subscriptions = new();
        private readonly string _instanceId;

        public RabbitUserSubscriptionService(IMessageClient messageClient, ILogger logger, IHttpContextAccessor httpContextAccessor,
            IUserQueueMessageHandler userQueueMessageHandler, IConfiguration configuration)
        {
            _messageClient = messageClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _userQueueMessageHandler = userQueueMessageHandler;

            var instance = configuration.GetSection("Instance").Get<Instance>();
            _instanceId = instance.Id;
        }

        private string GetSubscriptionKey(string sessionId, string organization, string @group, string user)
        {
            return $"{sessionId}.{organization}.{group}.{user}";
        }

        private string GetRoutingKey(string organization, string @group, string user)
        {
            return $"{organization}.{group}.{user}";
        }

        private string GetQueueName(string organization, string @group, string user)
        {
            var queueName = $"{_instanceId}-{organization}.{user}";
            return queueName;
        }

        private void EnsureRegistrationCredentials(string organization, string group, string user, string sessionId)
        {
            if (string.IsNullOrWhiteSpace(organization))
            {
                throw new ArgumentException("Organization is invalid.", nameof(organization));
            }

            if (string.IsNullOrWhiteSpace(user))
            {
                throw new ArgumentException("User is invalid.", nameof(user));
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("Id is invalid.", nameof(sessionId));
            }
        }

        /// <summary>
        /// This should only be called once when user is logged in
        /// </summary>
        /// <param name="organization"></param>
        /// <param name="user"></param>
        /// <param name="sessionId"></param>
        /// <param name="group"></param>
        public void Register(string organization, string user, string sessionId, string group = "*")
        {
            _logger.Information($"=============================  RabbitListener.Register({organization}, {user}, {sessionId}, {group}) Begin =========================");

            EnsureRegistrationCredentials(organization, group, user, sessionId);

            try
            {
                // WARNING: this method should only get called once when user is registered and logged in.
                // if it is registered already, skip it.
                var queueName = GetQueueName(organization, @group, user);
                var routingKey = GetRoutingKey(organization, @group, user);

                _logger.Information($"Registering rabbitMQ message listeners for user id '{user}' with session id '{sessionId}', queue name '{queueName}', routing key '{routingKey}'");

                var key = GetSubscriptionKey(sessionId, organization, group, user);
                if (!_subscriptions.ContainsKey(key))
                {
                    var subscription = _messageClient.Subscribe(MessageExchanges.UserExchange, queueName, new [] { routingKey }, ProcessMessage);
                    _subscriptions[key] = subscription;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to register message listeners", ex);
            }

            _logger.Information($"=============================  RabbitListener.Register({organization}, {user}, {sessionId}, {group}) End =========================");
        }

        /// <summary>
        /// Generic listener
        /// </summary>
        public void Register()
        {
            _logger.Information("=============================  RabbitListener.Register() Begin =========================");

            string organization = "*";
            string userName = "*";
            string sessionId = "*";

            Register(organization, userName, sessionId);
            _logger.Information("=============================  RabbitListener.Register() End =========================");
        }


        public void Register(JwtSecurityToken jwtSecurityToken)
        {
            _logger.Information("=============================  RabbitListener.Register(JwtSecurityToken jwtSecurityToken) Begin =========================");

            string organization = jwtSecurityToken.GetOrganization();
            string userName = jwtSecurityToken.GetId();
            string sessionId = jwtSecurityToken.GetJti();

            if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(sessionId))
            {
                return;
            }

            Register(organization, userName, sessionId);
            _logger.Information("=============================  RabbitListener.Register(JwtSecurityToken jwtSecurityToken) End =========================");
        }

        public void Deregister(string organization, string user, string sessionId, string group = "*")
        {
            _logger.Information("=============================  RabbitListener.Deregister() Begin =========================");

            EnsureRegistrationCredentials(organization, group, user, sessionId);

            _logger.Information($"Deregistering rabbitMQ message listeners for organization '{organization}', user '{user}' with sessionId '{sessionId}'");
            var key = GetSubscriptionKey(sessionId, organization, group, user);
            try
            {
                if (_subscriptions.ContainsKey(key))
                {
                    _subscriptions[key].Unsubscribe();
                    _subscriptions.TryRemove(key, out _);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to register message listeners", ex);
            }

            _logger.Information("=============================  RabbitListener.Deregister() End =========================");
        }

        public void Deregister()
        {
            var jwtSecurityToken = SecurityUtils.GetJwtToken(_httpContextAccessor.HttpContext);
            if (jwtSecurityToken == null)
            {
                return;
            }
            string organization = jwtSecurityToken.GetOrganization();
            string userName = jwtSecurityToken.GetId();
            string sessionId = jwtSecurityToken.GetJti();

            if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(sessionId))
            {
                return;
            }

//            string group = claim.GetGroup() ?? "*";
            string group = "*";

            Deregister(organization, userName, sessionId, group);
        }

        private async Task ProcessMessage(Message message)
        {
            _logger.Information($"RabbitUser Subscription received {message}");
            await _userQueueMessageHandler.Handle(message);
        }
    }
}
