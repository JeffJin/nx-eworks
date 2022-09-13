using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;

namespace adworks.data_services
{
    public interface IOrganizationService
    {
        Task<Guid> AddOrganization(OrganizationDto organization);
        
        bool DeleteOrganization(Guid id);

        Task<Organization> FindOrganization(Guid id);
        
        Task<Organization> FindOrganizationByName(string name);

        Task<IEnumerable<Organization>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn", bool isDescending = true);

        Task<Organization> UpdateOrganization(OrganizationDto deviceDto);

        Task<int> AddUser(string orgName, string email);
    }
}