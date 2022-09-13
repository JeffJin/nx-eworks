using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class VideoService : IVideoService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public VideoService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Video> AddVideo(VideoDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var model = DtoHelper.Convert(dto);
                var user = await dbContext.Users.Where(u => u.Email == dto.CreatedBy).SingleOrDefaultAsync();
                if (user != null)
                {
                    model.CreatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(dto.CreatedBy);
                }

                Category category = await dbContext.Categories.SingleOrDefaultAsync(c => c.Name == dto.Category);
                model.Category = category;
                var result = await dbContext.Videos.AddAsync(model);
                await dbContext.SaveChangesAsync();
                return result.Entity;
            }
        }

        public bool DeleteVideo(Guid videoId)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var video = dbContext.Videos.Find(videoId);
                if (video == null)
                {
                    return false;
                }
                else
                {
                    dbContext.Videos.Remove(video);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<Video> FindVideo(Guid videoId)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var video = await dbContext.Videos.Where(v => v.Id == videoId)
                    .Include(x => x.Category)
                    .Include(x => x.CreatedBy)
                    .SingleOrDefaultAsync();
                return video;
            }
        }

        public async Task<IEnumerable<Video>> FindVideos(
            string email,
            string category = "",
            int pageIndex = 0,
            int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn",
            bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Video> videos;

                Expression<Func<Video, bool>> whereLamda = string.IsNullOrEmpty(category) ?
                        (v) => v.CreatedBy.Email == email && v.Category.Name != "Unprocessed" :
                        (v) => v.CreatedBy.Email == email && v.Category.Name == category;
                if (isDescending)
                {
                    videos = orderBy switch
                    {
                        "Duration" => await dbContext.Videos.AsQueryable()
                            .Include(x => x.Category)
                            .Include(x => x.CreatedBy)
                            .Where(whereLamda)
                            .OrderByDescending(x => x.Duration)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                        "UpdatedOn" => await dbContext.Videos.AsQueryable()
                            .Include(x => x.Category)
                            .Include(x => x.CreatedBy)
                            .Where(whereLamda)
                            .OrderByDescending(x => x.UpdatedOn)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                        "Title" => await dbContext.Videos.AsQueryable()
                            .Include(x => x.Category)
                            .Include(x => x.CreatedBy)
                            .Where(whereLamda)
                            .OrderByDescending(x => x.Title)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                        _ => await dbContext.Videos.AsQueryable()
                            .Include(x => x.Category)
                            .Include(x => x.CreatedBy)
                            .Where(whereLamda)
                            .OrderByDescending(x => x.CreatedOn)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync()
                    };
                }
                else
                {
                    videos = orderBy switch
                    {
                        "Duration" => await dbContext.Videos.AsQueryable()
                            .Include(x => x.Category)
                            .Include(x => x.CreatedBy)
                            .Where(whereLamda)
                            .OrderBy(x => x.Duration)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                        "UpdatedOn" => await dbContext.Videos.AsQueryable()
                            .Include(x => x.Category)
                            .Include(x => x.CreatedBy)
                            .Where(whereLamda)
                            .OrderBy(x => x.UpdatedOn)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                        "Title" => await dbContext.Videos.AsQueryable()
                            .Include(x => x.Category)
                            .Include(x => x.CreatedBy)
                            .Where(whereLamda)
                            .OrderBy(x => x.Title)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                        _ => await dbContext.Videos.AsQueryable()
                            .Include(x => x.Category)
                            .Include(x => x.CreatedBy)
                            .Where(whereLamda)
                            .OrderBy(x => x.CreatedOn)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                    };
                }

                return videos;
            };
        }

        public async Task<IEnumerable<Video>> FindAll(
            string category = "",
            int pageIndex = 0,
            int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn",
            bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                Expression<Func<Video, bool>> whereLamda = string.IsNullOrEmpty(category) ?
                    (v) => v.Category.Name != "Unprocessed" :
                    (v) => v.Category.Name == category;
                if (isDescending)
                {
                    return orderBy switch
                    {
                        "Duration" => await dbContext.Videos.AsQueryable()
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category)
                            .Where(whereLamda)
                            .OrderByDescending(x => x.Duration)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                        "UpdatedOn" => await dbContext.Videos.AsQueryable()
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category)
                            .Where(whereLamda)
                            .OrderByDescending(x => x.UpdatedOn)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                        "Title" => await dbContext.Videos.AsQueryable()
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category)
                            .Where(whereLamda)
                            .OrderByDescending(x => x.Title)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                        _ => await dbContext.Videos.AsQueryable()
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category)
                            .Where(whereLamda)
                            .OrderByDescending(x => x.CreatedOn)
                            .Skip(skip)
                            .Take(pageSize)
                            .ToListAsync(),
                    };
                }

                return orderBy switch
                {
                    "Duration" => await dbContext.Videos.AsQueryable()
                        .Include(x => x.CreatedBy)
                        .Include(x => x.Category)
                        .Where(whereLamda)
                        .OrderBy(x => x.Duration)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync(),
                    "UpdatedOn" => await dbContext.Videos.AsQueryable()
                        .Include(x => x.CreatedBy)
                        .Include(x => x.Category)
                        .Where(whereLamda)
                        .OrderBy(x => x.UpdatedOn)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync(),
                    "Title" => await dbContext.Videos.AsQueryable()
                        .Include(x => x.CreatedBy)
                        .Include(x => x.Category)
                        .Where(whereLamda)
                        .OrderBy(x => x.Title)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync(),
                    _ => await dbContext.Videos.AsQueryable()
                        .Include(x => x.CreatedBy)
                        .Include(x => x.Category)
                        .Where(whereLamda)
                        .OrderBy(x => x.CreatedOn)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToListAsync(),
                };
            };
        }

        public async Task<Video> UpdateVideo(VideoDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = await dbContext.Videos.FindAsync(dto.Id);
                var cat = await dbContext.Categories.SingleOrDefaultAsync(c => c.Name == dto.Category);
                if (temp == null)
                {
                    throw new VideoNotFoundException(dto.Id);
                }
                temp.Description = dto.Description;
                temp.ProgressiveVideoUrl = dto.ProgressiveUrl;
                temp.ThumbnailLink = dto.MainThumbnail;
                temp.Title = dto.Title;
                temp.Tags = dto.Tags;
                temp.UpdatedOn = DateTimeOffset.UtcNow;
                temp.VodVideoUrl = dto.CloudUrl;
                if (cat != null)
                {
                    temp.Category = cat;
                }
                var user = await dbContext.Users.Where(u => u.Email == dto.UpdatedBy).SingleOrDefaultAsync();
                if (user != null)
                {
                    temp.UpdatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(dto.UpdatedBy);
                }
                await dbContext.SaveChangesAsync();
                return temp;
            }
        }

        public async Task<Video> AddOrUpdateVideo(VideoDto videoData)
        {
            if (videoData.Id == Guid.Empty)
            {
                return await AddVideo(videoData);
            }
            var video = await FindVideo(videoData.Id);
            if (video == null)
            {
                return await AddVideo(videoData);
            }
            else
            {
                return await UpdateVideo(videoData);
            }
        }

        public async Task<IEnumerable<Video>> FindUnprocessedVideos(string email)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                IEnumerable<Video> videos = await dbContext.Videos
                    .Include(v => v.Category)
                    .Where(v => v.CreatedBy.Email == email && v.Category.Name == "Unprocessed")
                    .OrderByDescending(x => x.Duration)
                    .ToListAsync();

                return videos;
            };
        }
    }
}
