using System;
using System.Collections.Generic;
using adworks.data_services.Identity;

namespace adworks.data_services.DbModels
{
    public class Order: Entity
    {
        public double Discount { get; set; }
        public User Customer { get; set; }
        public ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();
    }
}