using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace adworks.data_services.DbModels
{
    public class SubCategory
    {
        [Key]
        public Guid Id { get; set; }
        
        [Column(TypeName = "varchar(256)")]
        [Required]
        [MinLength(3)]
        public string Name { get; set; }    
        
        [JsonIgnore]
        [Required]
        public Category Category { get; set; }
    }
}