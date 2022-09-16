using System.Collections.Generic;

namespace adworks.media_common
{
    public class ProductDto : EntityDto
    {
        public string Title { get; set; }   
        public string Description { get; set; }   
        public double Price { get; set; }   
        public string Brand { get; set; }   
        public int Inventory { get; set; }  
        public ICollection<string> Images { get; set; }  
        public ICollection<string> Videos { get; set; }  
        public string Category { get; set; }  
        public string SubCategory { get; set; }
    }
}