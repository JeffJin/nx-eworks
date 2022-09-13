using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace adworks.data_services.DbModels
{
    public class OrderItem: Entity
    {
        [JsonIgnore]
        [Required]
        public Order Order { get; set; }
        public Product Product { get; set; }
        public int OrderQuantity { get; set; }
    }
}