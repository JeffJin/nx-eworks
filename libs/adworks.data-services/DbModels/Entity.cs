using System;
using System.ComponentModel.DataAnnotations;
using adworks.data_services.Identity;

namespace adworks.data_services.DbModels
{
    public abstract class Entity
    {
        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTimeOffset.UtcNow;
        }
        
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public DateTimeOffset CreatedOn { get; set; }
        
        public DateTimeOffset? UpdatedOn { get; set; }
        
        public User UpdatedBy { get; set; }
        
        [Required]
        public User CreatedBy { get; set; }
    }
}