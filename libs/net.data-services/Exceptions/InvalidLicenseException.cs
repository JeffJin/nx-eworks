using System;

namespace adworks.data_services
{
    public class InvalidLicenseException : Exception
    {
        public string DeviceSerialNumber { get; }

        public InvalidLicenseException(string serialNumber)
        {
            DeviceSerialNumber = serialNumber;
        }
    }
}