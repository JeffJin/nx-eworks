using System;

namespace adworks.pi_player.Exceptions
{
    public class FailedToDownloadException: Exception
    {
        public FailedToDownloadException(string message): base(message)
        {
        }
        
    }
}