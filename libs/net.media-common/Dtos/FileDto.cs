using System;

namespace adworks.media_common
{
    [Serializable]
    public class FileDto: EntityDto
    {
        public string RawFilePath { get; set; }
        public long FileSize { get; set; }
    }
}