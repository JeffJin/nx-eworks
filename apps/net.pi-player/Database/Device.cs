using System;
using System.ComponentModel.DataAnnotations;

namespace adworks.pi_player
{
    public class Device
    {
        public Device()
        {
            CreatedOn = DateTimeOffset.UtcNow;
        }
        
        [Key]
        public Guid Id { get; set; }

        // serialized data content
        public string DeviceInfo { get; set; }
        
        public DateTimeOffset CreatedOn { get; set; }
    }
}