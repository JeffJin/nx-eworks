using System;

namespace adworks.media_common
{
    [Serializable]
    public class MediaAssetDto : FileDto
    {
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public string CloudUrl { get; set; }
        public string FileType { get; set; }
    }
}