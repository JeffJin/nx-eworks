using System;
using System.Collections.Generic;

namespace adworks.media_common
{
    [Serializable]
    public class DeviceDto: EntityDto
    {        
        public string Locale { get; set; }
        public string Address { get; set; }
        public double? TimezoneOffset { get; set; }
        public string SerialNumber { get; set; }
        public string DeviceGroupName{ get; set; }
        public string OrganizationName{ get; set; }
        public string AssetTag { get; set; }
        public int DeviceVersion { get; set; }
        public int AppVersion { get; set; }
        public Guid? DeviceGroupId { get; set; }
        public Guid? LocationId { get; set; }
        public DateTimeOffset? ActivatedOn { get; set; }
        public DeviceStatusDto LastStatus { get; set; }
        
        public ICollection<LicenseDto> Licenses { get; } = new List<LicenseDto>();
    }
}