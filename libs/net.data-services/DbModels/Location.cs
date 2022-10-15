using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace adworks.data_services.DbModels
{
    public class Location : Entity
    {
        [Required]
        public string Address { get; set; }
        public string? Locale { get; set; }
        [Required]
        public double TimezoneOffset { get; set; }

        public ICollection<Device>? Devices { get; } = new List<Device>();
        public ICollection<Appointment>? Appointments { get; } = new List<Appointment>();

    }
}
