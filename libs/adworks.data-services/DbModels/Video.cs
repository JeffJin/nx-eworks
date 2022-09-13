using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace adworks.data_services.DbModels
{
    public class Video: MediaAsset
    {
        public double Duration { get; set; }

        [Column(TypeName = "varchar(512)")]
        public string ThumbnailLink { get; set; }

        [Column(TypeName = "varchar(512)")]
        public string VodVideoUrl { get; set; }
        
        [Column(TypeName = "varchar(512)")]
        public string EncodedVideoPath { get; set; }
        
        [Column(TypeName = "varchar(512)")]
        public string ProgressiveVideoUrl { get; set; }
    }
}