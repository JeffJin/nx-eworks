using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using adworks.media_common;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly ISystemService _systemService;
        private readonly ILogger _logger;
        private const int BUFFER_SIZE = 131072;

        public DownloadService(ISystemService systemService, ILogger logger)
        {
            _systemService = systemService;
            _logger = logger;
        }


        public async Task<bool> DownloadAndSaveFile(MediaAssetDto assetDto, string destination, Action<long, DeviceDto, MediaAssetDto> callback = null)
        {
            HttpClient client = new HttpClient();
            try
            {
                var osType = _systemService.GetOsType();
                var serialNumber = _systemService.GetSerialNumber(osType);
                var deviceDto = await _systemService.GetDeviceInfo(serialNumber);

                using (HttpResponseMessage response = client.GetAsync(assetDto.CloudUrl, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    response.EnsureSuccessStatusCode();

                    var totalRead = 0L;
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                        fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE, true))
                    {
                        var buffer = new byte[BUFFER_SIZE];
                        var isMoreToRead = true;

                        do
                        {
                            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0)
                            {
                                isMoreToRead = false;
                            }
                            else
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;
                            }

                            if (callback != null && totalRead > 0)
                            {
                                _logger.Information(string.Format("total bytes downloaded so far: {0:n0}", totalRead));
                                callback.Invoke(totalRead, deviceDto, assetDto);
                            }
                        }
                        while (isMoreToRead);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to download media from {url}", assetDto.CloudUrl);
                return false;
            }
        }
    }
}
