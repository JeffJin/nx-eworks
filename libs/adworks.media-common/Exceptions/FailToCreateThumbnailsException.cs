using System;

namespace adworks.media_common
{
    public class FailToCreateThumbnailsException : Exception
    {
        public Exception Reason { get; }

        public FailToCreateThumbnailsException(Exception ex)
        {
            Reason = ex;
        }
    }
}