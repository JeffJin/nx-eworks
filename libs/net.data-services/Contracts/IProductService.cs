using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;

namespace adworks.data_services
{
    public interface IProductService
    {
        Task<Guid> AddProduct(ProductDto productDto);
        
        bool DeleteProduct(Guid id);

        Task<Product> FindProduct(Guid id);

        Task<IEnumerable<Product>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn", bool isDescending = true);

        Task<Product> UpdateProduct(ProductDto productDto);
    }
}