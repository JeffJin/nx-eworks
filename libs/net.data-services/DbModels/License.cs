using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace adworks.data_services.DbModels
{
    public class License: Entity
    {
        [JsonIgnore]
        public Device? Device { get; set; }

        public string? Type { get; set; } //Enterprise, Individual, Trial, Normal

        public DateTimeOffset ExpireOn { get; set; }
    }
}
