using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace adworks.data_services.DbModels
{
    public class DeviceStatus: Entity
    {
        [Required]
        [JsonIgnore]
        public Device Device { get; set; }
        public string? Status { get; set; } //Online, Offline, PendingPayment, Expired
    }
}
