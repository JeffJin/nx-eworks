using System.Threading.Tasks;

namespace adworks.message_bus
{
    public interface IEventHub
    {
        Task Receive(string clientId, string message);
        Task Send(string clientId, string message);
        Task SendToCurrent(string message);
        Task Broadcast(string message);
        Task SendToGroup(string group, string message);
    }
}