using System;

namespace adworks.media_common
{
    public class FailedToTransferToFtpSiteException : Exception
    {
        public string Reason { get; }
        public Exception ExceptionSource { get; }

        public FailedToTransferToFtpSiteException(string msg, Exception exception)
        {
            Reason = msg;
            ExceptionSource = exception;
        }
        
        public FailedToTransferToFtpSiteException(string msg)
        {
            Reason = msg;
        }
    }
}