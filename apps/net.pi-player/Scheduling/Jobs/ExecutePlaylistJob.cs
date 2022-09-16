using System.IO;
using System.Linq;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.pi_player.Services;
using Quartz;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Scheduling.Jobs
{
    [DisallowConcurrentExecution()]
    public class ExecutePlaylistJob : IJob
    {
        private readonly IShellService _shell;
        private readonly ISystemService _systemService;
        private readonly ICachingService _cachingService;
        private readonly IDataService _dataService;
        private ILogger _logger;

        public ExecutePlaylistJob(IShellService shell, ISystemService systemService, ICachingService cachingService, IDataService dataService)
        {
            _shell = shell;
            _systemService = systemService;
            _cachingService = cachingService;
            _dataService = dataService;
            _logger = Log.Logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobKey key = context.JobDetail.Key;

            JobDataMap dataMap = context.MergedJobDataMap;

            var playlistDto = (PlaylistDto) dataMap[key.Name];

            foreach (var subList in playlistDto.SubPlaylists)
            {
                foreach (var item in subList.PlaylistItems)
                {
                    if (!File.Exists(item.CacheLocation))
                    {
                        var filePath = await _cachingService.DownloadAndSave(item);
                        item.CacheLocation = filePath;

                        await _dataService.SavePlaylist(playlistDto);
                    }

                    PlayAsset(item, subList.Width, subList.Height, subList.PositionX, subList.PositionY);
                }
            }
        }


        private string PlayAsset(PlaylistItemDto item, int width, int height, int posX, int posY)
        {
            _logger.Information("Start playing videos for playlist item {url}, {cache}", item.Media.CloudUrl,
                item.CacheLocation);

            var resolution = _systemService.GetScreenResolution();
            _logger.Information("Current screen resolution {x}x{y}", resolution.W, resolution.H);
            int w = width * resolution.W / 100;
            int h = height * resolution.H / 100;
            string result = string.Empty;

            if (item.AssetDiscriminator == AssetTypes.Video)
            {
                result = _shell.PlayVideo(item.CacheLocation, posX, posY, w, h, item.Duration * 1000);
            }
            else if (item.AssetDiscriminator == AssetTypes.Audio)
            {
                result = _shell.PlayAudio(item.CacheLocation);
            }
            else if (item.AssetDiscriminator == AssetTypes.Image)
            {
                result = _shell.ShowImage(item.CacheLocation, posX, posY, w, h);
            }

            _logger.Information("Finished playing asset {url}, {result}", item.Media.CloudUrl, result);

            return result;
        }
    }
}
