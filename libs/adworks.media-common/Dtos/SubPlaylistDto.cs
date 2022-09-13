using System;
using System.Collections.Generic;

namespace adworks.media_common
{
    [Serializable]
    public class SubPlaylistDto: EntityDto
    {
        public Guid PlaylistId { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        public ICollection<PlaylistItemDto> PlaylistItems { get; set; } = new List<PlaylistItemDto>();

    }
}