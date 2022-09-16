using System;
using System.ComponentModel.DataAnnotations;

namespace adworks.pi_player
{
    public class Playlist
    {
        public Playlist()
        {
            CreatedOn = DateTimeOffset.UtcNow;
        }
        
        [Key]
        public Guid Id { get; set; }

        // serialized data content
        public string Content { get; set; }
        
        public DateTimeOffset CreatedOn { get; set; }
    }
}