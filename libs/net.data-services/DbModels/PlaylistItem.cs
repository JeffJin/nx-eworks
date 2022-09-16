using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace adworks.data_services.DbModels
{
    public class PlaylistItem: Entity
    {
        [JsonIgnore]
        [Required]
        public SubPlaylist SubPlaylist { get; set; }
        [Required]
        public Guid MediaAssetId { get; set; }
        [Required]
        public string AssetDiscriminator { get; set; }
        public int Duration { get; set; }
        public int Index { get; set; }
    }
}