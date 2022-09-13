using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using adworks.data_services.Identity;

namespace adworks.data_services.DbModels
{
    /// <summary>
    /// many to many join group for playlist and device group
    /// </summary>
    public class PlaylistGroup: Entity
    {
        public PlaylistGroup()
        {
        }

        [Required]
        public DeviceGroup DeviceGroup { get; set; }
        
        [Required]
        public Playlist Playlist { get; set; }

    }
}