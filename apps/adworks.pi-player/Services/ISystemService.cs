using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.media_common;

namespace adworks.pi_player.Services
{
    public interface ISystemService
    {
        string GetOsType();
        IList<string> GetMacAddresses(string osType);
        string GetSerialNumber(string systemType);
        Task<DeviceDto> RequestProvision(string serialNumber);
        Task ReportStatus(DeviceDto deviceDto);
        Task<DeviceDto> GetDeviceInfo(string serialNumber);
        Resolution GetScreenResolution();
        Task<LicenseDto> ValidateLicense();
    }
}