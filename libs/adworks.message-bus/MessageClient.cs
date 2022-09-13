using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.message_common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.message_bus
{
    public class MessageClient : IMessageClient
    {
        private readonly IConnection _connection;
        private readonly ILogger _logger;

        public MessageClient(ILogger logger, IConnectionFactory connectionFactory)
        {
            _logger = logger;
            _connection = connectionFactory.CreateConnection();
        }

        // open the channel for the entire lifetime for performance reason
        public IModel CreateChannel()
        {
            var channel = _connection.CreateModel();
            return channel;
        }

        public void Publish(Message message, string exchangeName, string routingKey)
        {
            using (IModel channel = CreateChannel())
            {
                 Publish(message, exchangeName, routingKey, channel);
            }
        }

        /// <summary>
        /// Message entity will provide routing key as follows
        /// example 1: eworkspace.basement.J889900KLL777
        /// example 2: eworkspace.30dd879c-ee2f-11db-8314-0800200c9a66.123458437584932758943
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exchangeName">Exchange name, such as Device or User</param>
        /// <param name="routingKey">routing key</param>
        /// <param name="channel"></param>
        public void Publish(Message message, string exchangeName, string routingKey, IModel channel)
        {
            lock (channel)
            {
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                _logger.Information(" [*] Publishing for message on exchange '{0}', routing key '{1}, {2}'.",
                    exchangeName, routingKey, message);

                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);

                byte[] body = SerializeHelper.Serialize(message);
                channel.BasicPublish(exchange: exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);
            }
        }

        /// <summary>
        /// Per device per queue, or per logged in user browser session per queue
        /// </summary>
        /// <param name="exchange">Exchange name: Device or User</param>
        /// <param name="queueName">Dedicated queue name: Org1, Org1.Group1, Org1.User1, Org1.Group1.SerialNumber1</param>
        /// <param name="bindingKeys">Binding keys in wildcard format: ["Org1.Group2.*", "Org1.Group1.*"]</param>
        /// <param name="handleAction">message hanlder function wrapper</param>
        /// <param name="autoDelete"></param>
        public ISubscription Subscribe(string exchange, string queueName, string[] bindingKeys, Func<Message, Task> handleAction, bool autoDelete = false)
        {
            var channel = CreateChannel();
            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true, autoDelete: autoDelete);
            channel.QueueDeclare(queueName, true, false, autoDelete, null);

            foreach (var bindingKey in bindingKeys)
            {
                channel.QueueBind(queue: queueName, exchange: exchange, routingKey: bindingKey);
            }

            _logger.Information(" [*] Waiting for message on exchange '{0}', queue '{1}', binding keys '{2}'.",
                exchange, queueName, bindingKeys);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var message = SerializeHelper.Deserialize<Message>(ea.Body.ToArray());

                _logger.Debug("Receive message from routing key {0}, msg: {1}", ea.RoutingKey, ea.Body);

                await handleAction(message);

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: queueName,
                autoAck: false,
                consumer: consumer);

            // TODO figure out a way to actively clean up the dangling subscriptions
            return new RabbitChannelSubscription(channel);
        }

        public void Dispose()
        {
            try
            {
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to dispose connection and channel model", ex);
            }
        }
    }
}
