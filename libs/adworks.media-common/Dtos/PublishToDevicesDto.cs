using System;

namespace adworks.media_common
{
    public class PublishToDevicesDto
    {
        public Guid PlaylistId { get; set; }
        public string[] SerialNumbers { get; set; }
    }
}