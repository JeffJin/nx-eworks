using System;
using System.Threading.Tasks;
using adworks.media_common;

namespace adworks.pi_player.Services
{
    public interface IDownloadService
    {
        Task<bool> DownloadAndSaveFile(MediaAssetDto assetDto, string destination, Action<long, DeviceDto, MediaAssetDto> callback = null);
    }
}