using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;

namespace adworks.media_processor
{
    public class AdSuggeestionService: IAdSuggeestionService
    {
        public Task<IList<Video>> FindVideosForCustomer(string inputsEmail)
        {
            throw new System.NotImplementedException();
        }
    }
}