using System;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.message_common;
using RabbitMQ.Client;

namespace adworks.message_bus
{
    public interface IMessageClient: IDisposable
    {
        IModel CreateChannel();
        void Publish(Message message, string exchangeName, string routingKey);
        void Publish(Message message, string exchangeName, string routingKey, IModel channel);
        ISubscription Subscribe(string exchange, string queueName, string[] bindingKeys, Func<Message, Task> handleAction, bool autoDelete = false);
    }
}