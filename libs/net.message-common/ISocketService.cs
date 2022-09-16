using System.Threading.Tasks;

namespace adworks.message_common
{
    public interface ISocketService
    {
        Task Connect(string url);
        Task Send(object payload);
    }
}