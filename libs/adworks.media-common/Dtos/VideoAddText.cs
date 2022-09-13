using System;

namespace adworks.media_common
{
    public class VideoAddText: EntityDto
    {
        public Guid VideoId { get; set; }
        public string Text { get; set; }
        public string Color { get; set; }
        public int FontSize { get; set; }
    }
}