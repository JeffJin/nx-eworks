using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using adworks.data_services.DbModels;
using Newtonsoft.Json;

namespace adworks.data_services.DbModels
{
    public class Appointment: Entity
    {
        [Required]
        public string Name { get; set; }
        
        [Required, Phone]
        public string Phone { get; set; }
        
        [Required, EmailAddress]
        public string Email { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        
        public string Organizer { get; set; }
        [JsonIgnore]
        public Location Location { get; set; }
    }
}