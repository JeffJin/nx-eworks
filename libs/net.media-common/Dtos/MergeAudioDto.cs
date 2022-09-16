using System;

namespace adworks.media_common
{
    public class MergeAudioDto
    {
        public Guid AudioId { get; set; }
        public Guid ImageId { get; set; }
        public UserDto User { get; set; }
        public int Duration { get; set; }
    }
}