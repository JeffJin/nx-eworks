using System;

namespace adworks.data_services
{
    public class ImageNotFoundException : Exception
    {
        public Guid ImageId { get; }

        public ImageNotFoundException(Guid id)
        {
            ImageId = id;
        }
    }
}