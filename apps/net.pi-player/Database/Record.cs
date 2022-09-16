using System;
using System.ComponentModel.DataAnnotations;

namespace adworks.pi_player
{
    public class Record
    {
        public Record()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTimeOffset.UtcNow;
        }
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid MediaAssetId { get; set; }
        public Guid PlaylistItemId { get; set; }
        public Guid PlaylistId { get; set; }

        public string IpAddress { get; set; }
        public DateTimeOffset StartedOn { get; set; }
        public DateTimeOffset EndedOn { get; set; }
        
        public DateTimeOffset CreatedOn { get; set; }
    }
}
