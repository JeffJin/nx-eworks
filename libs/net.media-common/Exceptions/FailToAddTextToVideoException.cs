using System;

namespace adworks.media_common
{
    public class FailToAddTextToVideoException : Exception
    {
        public Exception Reason { get; }

        public FailToAddTextToVideoException(Exception ex)
        {
            Reason = ex;
        }
    }
}