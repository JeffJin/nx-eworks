using System.Threading.Tasks;
using adworks.media_common;

namespace adworks.pi_player.Scheduling
{
    public interface IPlaylistScheduler: IPlayerScheduler
    {
        Task SchedulePlaylist(PlaylistDto playlist);
        Task UnschedulePlaylist(PlaylistDto playlistToRemove);
    }
}