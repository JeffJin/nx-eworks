using System;

namespace adworks.media_common
{
    public class LicenseDto: EntityDto
    {
        public Guid? DeviceId { get; set; }
        public string Type { get; set; } //Enterprise, Individual, Trial, Normal
        public DateTimeOffset ExpireOn { get; set; }
    }
}