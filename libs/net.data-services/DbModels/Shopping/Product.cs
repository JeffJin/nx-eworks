using System;
using System.Collections.Generic;

namespace adworks.data_services.DbModels
{
    public class Product: Entity
    {
        public string Title { get; set; }   
        public string Description { get; set; }   
        public double Price { get; set; }   
        public string Brand { get; set; }   
        public int Inventory { get; set; }  
        
        public ICollection<Image> Images { get; } = new List<Image>();
        public ICollection<Video> Videos { get; } = new List<Video>();   

        public SubCategory ProductCategory { get; set; }   
    }
}