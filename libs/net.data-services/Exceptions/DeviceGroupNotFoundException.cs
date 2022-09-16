using System;

namespace adworks.data_services
{
    public class DeviceGroupNotFoundException : Exception
    {
        private readonly Guid _id;

        public DeviceGroupNotFoundException(Guid id)
        {
            _id = id;
        }
    }
}