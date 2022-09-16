using System;

namespace adworks.pi_player.Exceptions
{
    public class StatusReportException: Exception
    {
        public string SerialNumber { get; }

        public StatusReportException(string serialNumber)
        {
            SerialNumber = serialNumber;
        }
        

        public StatusReportException(string serialNumber, string message): base(message)
        {
            SerialNumber = serialNumber;
        }
        
    }
}