using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace adworks.data_services.DbModels
{
    public class DeviceGroup: Entity
    {
        [Required]
        public string Name { get; set; }

        [JsonIgnore]
        public Organization? Organization { get; set; }

        public ICollection<Device>? Devices { get; } = new List<Device>();
        public ICollection<PlaylistGroup>? PlaylistGroups { get; } = new List<PlaylistGroup>();

    }
}
