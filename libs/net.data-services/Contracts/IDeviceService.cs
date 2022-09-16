using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.data_services.Identity;
using adworks.media_common;

namespace adworks.data_services
{
    public interface IDeviceService
    {
        Task<Guid> RegisterDevice(DeviceDto device);

        bool DeleteDevice(Guid id);

        Task<Device> FindDevice(Guid id);

        Task<IEnumerable<Device>> FindDevicesByGroup(string groupName, int pageIndex = 0, int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn", bool isDescending = true);

        Task<IEnumerable<Device>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn", bool isDescending = true);

        Task<Device> UpdateDevice(DeviceDto deviceDto);

        Task<DeviceStatus> GetDeviceStatus(string serialNumber);

        Task<DeviceStatus> AddOrUpdateDeviceStatus(DeviceStatusDto statusDto);

        Task<bool> CheckLicense(string serialNumber);
        
        Task<int> RemoveDeviceStatus(Guid deviceId, int offsetDays);
        
        Task<Device> FindDeviceBySerialNumber(string serial);

        Task<IList<User>> GetAssociatedUsers(string serialNumber);
    }
}