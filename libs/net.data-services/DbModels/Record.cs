using System;
using System.ComponentModel.DataAnnotations;

namespace adworks.data_services.DbModels
{
    public class Record: Entity
    {
        [Required]
        public Guid MediaAssetId { get; set; }
        [Required]
        public string DeviceSerialNumber { get; set; }
        public string IpAddress { get; set; }
        public DateTimeOffset StartedOn { get; set; }
        public DateTimeOffset EndedOn { get; set; }
    }
}