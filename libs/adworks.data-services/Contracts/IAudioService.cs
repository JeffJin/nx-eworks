using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;

namespace adworks.data_services
{
    public interface IAudioService
    {
        Task<Audio> AddAudio(AudioDto dto);
        bool DeleteAudio(Guid id);
        Task<Audio> FindAudio(Guid videoId);
        Task<IEnumerable<Audio>> FindAudios(string email, int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn", bool isDescending = true);
        Task<Audio>  UpdateAudio(AudioDto video);
        Task<IEnumerable<Audio>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn", bool isDescending = true);
        Task<Audio> AddOrUpdateVideo(AudioDto dto);
    }
}