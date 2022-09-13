using System;

namespace adworks.data_services
{
    public class SubPlaylistNotFoundException : Exception
    {
        public Guid SubPlaylistId { get; }

        public SubPlaylistNotFoundException(Guid id)
        {
            SubPlaylistId = id;
        }
    }
}