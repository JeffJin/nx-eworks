using System;

namespace adworks.media_common
{
    [Serializable]
    public class DeviceStatusDto: EntityDto
    {        
        public string SerialNumber { get; set; }
        public Guid DeviceId{ get; set; }
        public string Status { get; set; }
        public DateTimeOffset ReceivedOn { get; set; }
    }
}