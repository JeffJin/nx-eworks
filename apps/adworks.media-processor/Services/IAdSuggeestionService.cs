using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;

namespace adworks.media_processor
{
    public interface IAdSuggeestionService
    {
        Task<IList<Video>> FindVideosForCustomer(string inputsEmail);
    }
}