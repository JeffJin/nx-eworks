using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using adworks.media_common;

namespace adworks.media_common
{
    [Serializable]
    public class DeviceGroupDto: EntityDto
    {
        public string Name { get; set; }
        public string OrganizationName { get; set; }
        public int NumOfDevices { get; set; }
        public int NumOfPlaylists { get; set; }
        
        public ICollection<DeviceDto> Devices { get; } = new List<DeviceDto>();
        public ICollection<PlaylistDto> Playlists { get; } = new List<PlaylistDto>();
    }
}