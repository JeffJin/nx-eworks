using System;

namespace adworks.data_services
{
    public class PlaylistItemNotFoundException : Exception
    {
        public Guid PlaylistItemId { get; }

        public PlaylistItemNotFoundException(Guid id)
        {
            PlaylistItemId = id;
        }
    }
}