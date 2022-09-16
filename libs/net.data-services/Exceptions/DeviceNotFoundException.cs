using System;

namespace adworks.data_services
{
    public class DeviceNotFoundException : Exception
    {
        public string SerialNumber { get; }
        public Guid DeviceId { get; }

        public DeviceNotFoundException(Guid id)
        {
            DeviceId = id;
        }
        public DeviceNotFoundException(string serialNumber)
        {
            SerialNumber = serialNumber;
        }
    }
}