using System;

namespace adworks.data_services
{
    public class PlaylistNotFoundException : Exception
    {
        public Guid PlaylistId { get; }

        public PlaylistNotFoundException(Guid playlistId)
        {
            PlaylistId = playlistId;
        }
    }
}