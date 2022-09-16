using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class ImageService : IImageService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public ImageService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Image> AddImage(ImageDto dto)
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
                var result = await dbContext.Images.AddAsync(model);
                await dbContext.SaveChangesAsync();
                return result.Entity;
            }

        }

        public bool DeleteImage(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var model = dbContext.Images.Find(id);
                if (model == null)
                {
                    return false;
                }
                else
                {
                    dbContext.Images.Remove(model);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<IEnumerable<Image>> FindAll(
            string category,
            int pageIndex = 0,
            int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn",
            bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Image> images;
                if (isDescending)
                {
                    if (orderBy == "UpdatedOn")
                    {
                        images = await dbContext.Images.OrderByDescending(x => x.UpdatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "Title")
                    {
                        images = await dbContext.Images.OrderByDescending(x => x.Title).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else
                    {
                        images = await dbContext.Images.OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }

                }
                else
                {
                    if (orderBy == "UpdatedOn")
                    {
                        images = await dbContext.Images.OrderBy(x => x.UpdatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "Title")
                    {
                        images = await dbContext.Images.OrderBy(x => x.Title).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else
                    {
                        images = await dbContext.Images.OrderBy(x => x.CreatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                }

                return images;
            };
        }

        public async Task<Image> FindImage(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var image = await dbContext.Images.Where(v => v.Id == id)
                    .Include(x => x.Category)
                    .Include(x => x.CreatedBy)
                    .SingleOrDefaultAsync();
                return image;
            }
        }

        public async Task<IEnumerable<Image>> FindImages(
            string email,
            string category,
            int pageIndex = 0,
            int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn",
            bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Image> images;

                if (isDescending)
                {
                    if (orderBy == "UpdatedOn")
                    {
                        images = await dbContext.Images.Where(x => x.CreatedBy.Email == email).OrderByDescending(x => x.UpdatedOn).Skip(skip).Take(pageSize).Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "Title")
                    {
                        images = await dbContext.Images.Where(x => x.CreatedBy.Email == email).OrderByDescending(x => x.Title).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else
                    {
                        images = await dbContext.Images.Where(x => x.CreatedBy.Email == email).OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }

                }
                else
                {
                    if (orderBy == "UpdatedOn")
                    {
                        images = await dbContext.Images.Where(x => x.CreatedBy.Email == email).OrderBy(x => x.UpdatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "Title")
                    {
                        images = await dbContext.Images.Where(x => x.CreatedBy.Email == email).OrderBy(x => x.Title).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else
                    {
                        images = await dbContext.Images.Where(x => x.CreatedBy.Email == email).OrderBy(x => x.CreatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                }

                return images;
            };
        }

        public async Task<Image> UpdateImage(ImageDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = await dbContext.Images.Include(v => v.Category).SingleOrDefaultAsync(v => v.Id == dto.Id);
                if (temp == null)
                {
                    throw new ImageNotFoundException(dto.Id);
                }
                temp.Description = dto.Description;
                temp.CloudUrl = dto.CloudUrl;
                temp.Title = dto.Title;
                temp.Tags = dto.Tags;
                temp.UpdatedOn = DateTimeOffset.UtcNow;
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

        public async Task<Image> AddOrUpdateImage(ImageDto dto)
        {
            if (dto.Id == Guid.Empty)
            {
                return await AddImage(dto);
            }
            var image = await FindImage(dto.Id);
            if (image == null)
            {
                return await AddImage(dto);
            }
            else
            {
                return await UpdateImage(dto);
            }
        }
    }
}
