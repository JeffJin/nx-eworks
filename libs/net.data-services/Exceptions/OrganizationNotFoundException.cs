using System;

namespace adworks.data_services
{
    public class OrganizationNotFoundException : Exception
    {
        public Guid OrganizationId { get; }

        public OrganizationNotFoundException(Guid organizationId)
        {
            OrganizationId = organizationId;
        }
    }
}