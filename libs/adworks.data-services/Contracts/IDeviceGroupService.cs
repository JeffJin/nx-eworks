using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;

namespace adworks.data_services
{
    public interface IDeviceGroupService
    {
        Task<Guid> AddGroup(DeviceGroupDto @group);

        bool DeleteDeviceGroup(Guid id);

        Task<DeviceGroup> FindGroup(Guid id);

        Task<IEnumerable<DeviceGroupDto>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn", bool isDescending = true);

        Task<DeviceGroup> UpdateDeviceGroup(DeviceGroupDto deviceDto);
        
        /// <summary>
        /// New device id will be returned
        /// </summary>
        /// <param name="deviceDto"></param>
        /// <returns>new device id</returns>
        Task<Guid> AddDeviceToGroup(DeviceDto deviceDto);

        Task<PlaylistGroup> AddPlaylistGroup(PlaylistGroupDto dto);
        Task<PlaylistGroup> FindPlaylistGroup(Guid playlistGroupId);
        Task<bool> RemovePlaylistGroup(PlaylistGroupDto dto);
        Task<IEnumerable<DeviceGroupDto>> GetGroupsWithDevices(int number = 4);
    }
}