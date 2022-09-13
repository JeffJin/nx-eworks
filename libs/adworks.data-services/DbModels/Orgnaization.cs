using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using adworks.data_services.Identity;

namespace adworks.data_services.DbModels
{
    public class Organization: Entity
    {
        [Required]
        public string Name { get; set; }
        
        public ICollection<DeviceGroup> DeviceGroups { get; } = new List<DeviceGroup>();
        public ICollection<User> Users { get; } = new List<User>();
    }
}