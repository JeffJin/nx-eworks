using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace adworks.data_services.DbModels
{
    public class Image: MediaAsset
    {
        [Column(TypeName = "varchar(512)")]
        public string? CloudUrl { get; set; }
    }
}
