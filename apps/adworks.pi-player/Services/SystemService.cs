using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
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
    public class SystemService : ISystemService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly IShellService _shellService;
        private readonly IDataService _dataService;
        private readonly IMessageClient _messageClient;

        public SystemService(IConfiguration config, ILogger logger, IShellService shellService,
            IDataService dataService, IMessageClient messageClient)
        {
            _config = config;
            _logger = logger;
            _shellService = shellService;
            _dataService = dataService;
            _messageClient = messageClient;
        }

        public string GetOsType()
        {
            string result = _shellService.Bash("uname -a");
            if (result.Contains("Darwin"))
            {
                return SystemTypes.Mac;
            }
            if (result.Contains("Linux") && result.Contains("raspberrypi"))
            {
                return SystemTypes.RaspberryPi;
            }
            if (result.Contains("Linux") && result.Contains("Ubuntu"))
            {
                return SystemTypes.Ubuntu;
            }
            if (result.Contains("MSYS_NT"))
            {
                return SystemTypes.Windows;
            }
            return null;
        }

        public IList<string> GetMacAddresses(string systemType)
        {
            IList<string> results = null;
            string result = "";
            if (systemType == SystemTypes.Mac)
            {
                result = _shellService.Bash("networksetup -listallhardwareports | awk '/Device: /{getline; print $3}'");
            }
            if (systemType == SystemTypes.RaspberryPi)
            {
                result = _shellService.Bash("ifconfig -a | grep -Po 'HWaddr \\K.*$'");
            }
            if (systemType == SystemTypes.Windows)
            {
                result = _shellService.Bash("getmac");
            }
            results = result.Split(Environment.NewLine);

            return results;
        }

        public string GetSerialNumber(string systemType)
        {
            string result = "";
            if (systemType == SystemTypes.Mac)
            {
                result = _shellService.Bash(@"system_profiler SPHardwareDataType | awk '/Serial Number \(processor tray\)/ {print $5}'");
            }
            if (systemType == SystemTypes.RaspberryPi)
            {
                result = _shellService.Bash("cat /proc/cpuinfo | grep Serial | cut -d ' ' -f 2");
            }
            if (systemType == SystemTypes.Windows)
            {
                result = "windows bios id";
            }

            return StringHelper.RemoveInvalidPathChars(result);
        }

        /// <summary>
        /// connect to server with device serial number to get device info and persist it into file system
        /// </summary>
        /// <returns>Device Info</returns>
        public async Task<DeviceDto> GetDeviceInfo(string serialNumber)
        {
            var device = await _dataService.GetDevice();
            if (device.SerialNumber == serialNumber)
            {
                return device;
            }

            _logger.Warning("Device serial number does not match the one saved ijn local database");
            return null;
        }

        /// <summary>
        /// connect to server with device serial number to get device info and persist it into file system
        /// </summary>
        /// <returns>Device Info</returns>
        public async Task<DeviceDto> RequestProvision(string serialNumber)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var result = await client.GetAsync(_config["ApiEndpoint"] + "device/request/" + serialNumber);

                    var deviceDto = JsonConvert.DeserializeObject<DeviceDto>(await result.Content.ReadAsStringAsync());
                    if (deviceDto != null)
                    {
                        await _dataService.SaveDevice(deviceDto);
                        return deviceDto;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to get system device info");
                }

                return null;
            }
        }

        /// <summary>
        /// use rabbitmq to do heartbeat
        /// </summary>
        /// <returns>Device Info</returns>
        public async Task ReportStatus(DeviceDto deviceDto)
        {
            var dto = new DeviceStatusDto()
            {
                SerialNumber = deviceDto.SerialNumber,
                Status = "Online",
                DeviceId = Guid.Empty,
                CreatedBy = _config["DefaultEmail"],
                UpdatedBy = "",
                CreatedOn = DateTimeOffset.UtcNow
            };
            var messageIdentity = new MessageIdentity(deviceDto.OrganizationName, deviceDto.DeviceGroupName, deviceDto.SerialNumber);

            _messageClient.Publish(new Message(messageIdentity)
            {
                Topic = MessageTopics.PiReportStatus,
                Body = SerializeHelper.Stringify(dto)
            }, MessageExchanges.ProcessorExchange, $"{messageIdentity.ToString()}.{MessageTopics.PiReportStatus}");
            await Task.CompletedTask;
        }

        public Resolution GetScreenResolution()
        {
            try
            {
                var res = _shellService.Bash("cat /sys/class/graphics/fb0/virtual_size");
                var temp = res.Split(",");
                int x = Convert.ToInt16(temp[0]);
                int y = Convert.ToInt16(temp[1]);
                return new Resolution(x, y);
            }
            catch
            {
                //fallback mechanism mostly for testing purpose
                return new Resolution(1920, 1080);
            }

        }

        public async Task<LicenseDto> ValidateLicense()
        {
            await Task.FromException<NotImplementedException>(new NotFiniteNumberException());
            return null;
        }
    }
}
