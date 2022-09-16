using System.Threading.Tasks;
using adworks.message_bus;

namespace adworks.pi_player.Processors
{
    public interface IProcessor
    {
        Task<ISubscription> Run();
    }
}