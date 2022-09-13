using System;

namespace adworks.data_services
{
    public class VideoNotFoundException : Exception
    {
        public Guid VideoId { get; }

        public VideoNotFoundException(Guid videoId)
        {
            VideoId = videoId;
        }
    }
}