using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace adworks.data_services.DbModels
{
    public class Playlist: Entity
    {
        public Playlist()
        {
            
        }
        [Required]
        public string Name { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        //daily start time in minutes, offset from midnight
        public int StartTime { get; set; }
        //daily end time in minutes, offset from midnight
        public int EndTime { get; set; }
        
        public ICollection<SubPlaylist> SubPlaylists { get; } = new List<SubPlaylist>();
        public ICollection<PlaylistGroup> PlaylistGroups { get; } = new List<PlaylistGroup>();
    }
}