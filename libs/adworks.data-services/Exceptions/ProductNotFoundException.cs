using System;

namespace adworks.data_services
{
    public class ProductNotFoundException : Exception
    {
        public Guid ProductId { get; }

        public ProductNotFoundException(Guid id)
        {
            ProductId = id;
        }
    }
}