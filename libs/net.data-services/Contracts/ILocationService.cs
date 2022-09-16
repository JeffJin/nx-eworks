using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;

namespace adworks.data_services
{
    public interface ILocationService
    {
        Task<Guid> AddLocation(LocationDto location);

        bool DeleteLocation(Guid id);

        Task<Location> FindLocation(Guid id);

        Task<IEnumerable<Location>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn", bool isDescending = true);

        Task<Location> UpdateLocation(LocationDto locationDto);

    }
}