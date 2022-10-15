using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace adworks.data_services.DbModels
{
    public class Audio: MediaAsset
    {
        public double Duration { get; set; }
        [Column(TypeName = "varchar(512)")]
        public string? CloudUrl { get; set; }
        [Column(TypeName = "varchar(512)")]
        public string? EncodedFilePath { get; set; }
    }
}
