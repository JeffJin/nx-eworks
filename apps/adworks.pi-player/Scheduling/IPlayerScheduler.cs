using System.Threading.Tasks;

namespace adworks.pi_player.Scheduling
{
    public interface IPlayerScheduler
    {
        Task Pause();
        Task Resume();
    }
}