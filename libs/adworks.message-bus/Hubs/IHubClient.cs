using System.Threading.Tasks;

namespace adworks.message_bus
{
    public interface IHubClient
    {
        Task Send(string message);
    }
}