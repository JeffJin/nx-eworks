using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;

namespace adworks.data_services
{
    public interface ICommonDataService
    {
        Task<Category> FindCategory(string name);
        Task<IEnumerable<Category>> GetCategories();
    }
}