using System;
using System.Collections.Generic;
using adworks.media_common;

namespace adworks.media_common
{
    [Serializable]
    public class PlaylistDto: EntityDto
    {
        public string Name { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        //daily start time in minutes, offset from midnight
        public int StartTime { get; set; }
        //daily end time in minutes, offset from midnight
        public int EndTime { get; set; }
        
        public ICollection<SubPlaylistDto> SubPlaylists { get; set; }
        public ICollection<DeviceGroupDto> DeviceGroups { get; set; }

        public PlaylistDto()
        {
            SubPlaylists = new List<SubPlaylistDto>();
            DeviceGroups = new List<DeviceGroupDto>();
        }
        
    }
}