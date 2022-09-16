using System;

namespace adworks.media_common
{
    public class FailToEncodeVideoException : Exception
    {
        public Exception Reason { get; }

        public FailToEncodeVideoException(Exception ex)
        {
            Reason = ex;
        }
    }
}