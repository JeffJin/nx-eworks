using System.IO;
using System.Threading.Tasks;
using adworks.media_common;

namespace adworks.pi_player.Services
{
    public interface ICachingService
    {
        Task<string> DownloadAndSave(PlaylistItemDto dto);
        void RemoveCache(PlaylistDto playlistDto);
    }
}