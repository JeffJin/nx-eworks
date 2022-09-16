using System;

namespace adworks.media_common
{
    public class PlaylistGroupDto
    {
        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset? UpdatedOn { get; set; }

        public string UpdatedBy { get; set; }

        public string CreatedBy { get; set; }
        
        public Guid PlaylistId { get; set; }
        public Guid DeviceGroupId { get; set; }
        public DeviceGroupDto DeviceGroup { get; set; }
        public PlaylistDto Playlist { get; set; }

    }
}