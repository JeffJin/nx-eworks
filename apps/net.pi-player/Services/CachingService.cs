using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.message_bus;
using adworks.message_common;
using adworks.pi_player.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Services
{
    public class CachingService: ICachingService
    {
        private readonly string CachingLocation = "cache/{0}/{1}"; //caching location, append playlist id and playlist item id
        private readonly IDownloadService _downloadService;
        private readonly IConfiguration _config;
        private readonly IMessageClient _messageClient;
        private readonly ILogger _logger;
        private readonly long _maxCacheSize; //max cache size is 1GB

        public CachingService(IDownloadService downloadService, IConfiguration config, ILogger logger,
            IMessageClient messageClient)
        {
            _downloadService = downloadService;
            _config = config;
            _messageClient = messageClient;
            _logger = logger;
            _maxCacheSize = Convert.ToInt64(_config["MaxCacheSize"]);
        }


        public async Task<string> DownloadAndSave(PlaylistItemDto dto)
        {
            string fileName = Path.GetFileName(dto.Media.RawFilePath);

            string dir = Path.Combine(_config["BaseAssetFolder"],
                string.Format(CachingLocation, dto.AssetDiscriminator, dto.MediaAssetId));

            if (!Directory.Exists(dir))
            {
                //create directory first
                Directory.CreateDirectory(dir);
            }
            string destination = Path.Combine(dir, fileName);
            //check if the file is already downloaded
            if (File.Exists(destination))
            {
                return destination;
            }

            //TODO implement retry mechanism
            var result = await _downloadService.DownloadAndSaveFile(dto.Media, destination, ReportProgress);

            if (!result)
            {
                throw new FailedToDownloadException("Unable to download media asset from " + dto.Media.CloudUrl);
            }
            return destination;
        }


        private static long DirSize(DirectoryInfo dirInfo)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = dirInfo.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = dirInfo.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        public void RemoveOldeCache(DirectoryInfo dirInfo)
        {
            //find the oldest cache and remove
            var directories = dirInfo.GetDirectories();
            directories = directories.OrderBy(d => d.LastWriteTime).ToArray();
            directories[0].Delete(true);
        }

        private void RemoveCache(DirectoryInfo dirInfo)
        {
            try
            {
                //find the oldest cache and remove
                var directories = dirInfo.GetDirectories();
                foreach (var d in directories)
                {
                    d.Delete(true);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to remove directory {dir}", dirInfo.Name);
            }

        }

        public void RemoveCache(PlaylistDto dto)
        {
            try
            {
                foreach (var sub in dto.SubPlaylists)
                {
                    foreach (var item in sub.PlaylistItems)
                    {
                        var dirInfo = new DirectoryInfo(item.CacheLocation);
                        dirInfo.Delete(true);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to remove playlist cache {playlist}", dto.Name);
            }

        }

        private void ReportProgress(long size, DeviceDto deviceDto, MediaAssetDto assetDto)
        {
            Task.Factory.StartNew(() =>
            {
                _logger.Information(string.Format("{0} bytes downloaded", size));
                try
                {
                    var payload = new ProgressPayload
                    {
                        DownloadedSize = size,
                        AssetTitle = assetDto.Title,
                        AssetUrl = assetDto.CloudUrl
                    };

                    var messageIdentity = new MessageIdentity(deviceDto.OrganizationName, deviceDto.DeviceGroupName, deviceDto.SerialNumber);

                    _messageClient.Publish(new Message(messageIdentity)
                    {
                        Topic = MessageTopics.PiDownloadAssetProgress,
                        Body = SerializeHelper.Stringify(payload)
                    }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}");
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Failed to report file download progress {url}", assetDto.CloudUrl);
                }
            });


        }
    }
}
