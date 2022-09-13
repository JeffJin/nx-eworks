using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;

namespace adworks.media_processor
{
    public class AdSearchEngine : IAdSearchEngine
    {
        private readonly IAdSuggeestionService _adSuggestionService;

        public AdSearchEngine(IAdSuggeestionService adSuggestionService)
        {
            _adSuggestionService = adSuggestionService;
        }


        public async Task<IEnumerable<VideoDto>> Search(AdSearchInput inputs)
        {
            IList<Video> results = await this._adSuggestionService.FindVideosForCustomer(inputs.Email);
            
            return await Task.FromResult(results.Select(r => DtoHelper.Convert(r)));
        }
    }
}