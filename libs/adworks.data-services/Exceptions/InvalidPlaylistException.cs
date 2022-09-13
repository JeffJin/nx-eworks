using System;

namespace adworks.data_services
{
    public class InvalidPlaylistException : Exception
    {
        public Guid PlaylistId { get; }

        public InvalidPlaylistException(Guid playlistId)
        {
            PlaylistId = playlistId;
        }
    }
}