using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.media_common;

namespace adworks.media_processor
{
    public interface IAdSearchEngine
    {
        Task<IEnumerable<VideoDto>> Search(AdSearchInput inputs);
    }
}