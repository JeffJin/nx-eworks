using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using adworks.data_services.Identity;

namespace adworks.data_services.DbModels
{
    public abstract class MediaAsset: Entity
    {
        [Column(TypeName = "varchar(512)")]
        public string Title { get; set; }
        
        [Column(TypeName = "varchar(512)")]
        public string Description { get; set; }
        
        [Column(TypeName = "varchar(512)")]
        [Required]
        public string RawFilePath { get; set; }
        
        [Column(TypeName = "varchar(512)")]
        public string Tags { get; set; }
        
        [Required]
        public Category Category { get; set; }
    }
}