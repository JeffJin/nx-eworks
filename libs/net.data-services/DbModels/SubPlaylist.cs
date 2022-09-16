using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace adworks.data_services.DbModels
{
    public class SubPlaylist: Entity
    {
        public SubPlaylist()
        {
            
        }
        
        [JsonIgnore]
        [Required]
        public Playlist Playlist { get; set; }
        
        //top left corner in screen
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        
        //player screen width and height
        public int Width { get; set; }
        public int Height { get; set; }
        
        public ICollection<PlaylistItem> PlaylistItems { get; } = new List<PlaylistItem>();
    }
}