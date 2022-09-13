using System;

namespace adworks.media_common
{
    public class FailedToEncodeAudioException : Exception
    {
        public Exception Reason { get; }

        public FailedToEncodeAudioException(Exception ex)
        {
            Reason = ex;
        }
    }
}