using System.Threading.Tasks;
using adworks.media_common;

namespace adworks.message_bus
{
    public interface IUserQueueMessageHandler
    {
        Task Handle(Message message);
    }
}