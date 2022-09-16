using System;

namespace adworks.data_services
{
    public class AudioNotFoundException : Exception
    {
        public Guid AudioId { get; }

        public AudioNotFoundException(Guid id)
        {
            AudioId = id;
        }
    }
}