using System;
using adworks.data_services.Identity;

namespace adworks.data_services.DbModels
{
    public class Payment: Entity
    {
        public Order? Order { get; set; }
        public string? TransactionId { get; set; }
    }
}
