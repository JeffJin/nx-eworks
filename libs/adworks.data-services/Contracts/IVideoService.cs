using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace adworks.data_services
{
    public interface IVideoService
    {
        Task<Video> AddVideo(VideoDto dto);
        bool DeleteVideo(Guid videoId);
        Task<Video> FindVideo(Guid videoId);
        Task<IEnumerable<Video>> FindVideos(
            string email, 
            string category = "", 
            int pageIndex = 0, 
            int pageSize = Int32.MaxValue, 
            string orderBy = "CreatedOn", 
            bool isDescending = true);
        Task<IEnumerable<Video>> FindAll(
            string category = "", 
            int pageIndex = 0, 
            int pageSize = Int32.MaxValue, 
            string orderBy = "CreatedOn", 
            bool isDescending = true);
        Task<Video> UpdateVideo(VideoDto video);
        Task<Video> AddOrUpdateVideo(VideoDto videoData);
        Task<IEnumerable<Video>> FindUnprocessedVideos(string email);
    }
}