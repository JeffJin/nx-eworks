using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace adworks.data_services.DbModels
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; }
        
        [Column(TypeName = "varchar(256)")]
        [Required]
        [MinLength(3)]
        public string Name { get; set; }  
        
        public ICollection<SubCategory> SubCategories { get; } = new List<SubCategory>();
    }
}