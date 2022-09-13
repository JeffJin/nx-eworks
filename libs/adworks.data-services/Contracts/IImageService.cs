using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;

namespace adworks.data_services
{
    public interface IImageService
    {
        Task<Image> AddImage(ImageDto dto);
        bool DeleteImage(Guid id);

        Task<IEnumerable<Image>> FindAll(string category = "",
            int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn",
            bool isDescending = true);

        Task<Image> FindImage(Guid id);

        Task<IEnumerable<Image>> FindImages(string email, string category = "",
            int pageIndex = 0, int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn", bool isDescending = true);

        Task<Image> UpdateImage(ImageDto dto);
        Task<Image> AddOrUpdateImage(ImageDto dto);
    }
}