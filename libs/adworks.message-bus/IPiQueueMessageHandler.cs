using adworks.media_common;

namespace adworks.message_bus
{
    public interface IPiQueueMessageHandler
    {
        void Handle(Message message);
    }
}