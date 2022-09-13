using System;

namespace adworks.media_common
{
    public class RemovePlaylistItemDto
    {
        public Guid PlaylistId { get; set; }
        public Guid SubPlaylistId { get; set; }
        public Guid PlaylistItemId { get; set; }
        public string UpdatedBy { get; set; }
        
    }
}