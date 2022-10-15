using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace adworks.data_services.DbModels
{
    public class Device: Entity
    {
        [Required]
        [MinLength(5)]
        [MaxLength(64)]
        public string SerialNumber { get; set; }
        public string? AssetTag { get; set; }
        public int DeviceVersion { get; set; }
        public int AppVersion { get; set; }
        public DateTimeOffset? ActivatedOn { get; set; }

        [JsonIgnore]
        public ICollection<License>? Licenses { get; } = new List<License>();

        [JsonIgnore]
        public DeviceGroup? DeviceGroup { get; set; }

        [JsonIgnore]
        public Location? Location { get; set; }
    }
}
