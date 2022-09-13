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
    public class AudioService: IAudioService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public AudioService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;

            _logger = logger;
        }

        public async Task<Audio> AddAudio(AudioDto dto)
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
                var result = dbContext.Audios.Add(model);
                await dbContext.SaveChangesAsync();

                return result.Entity;
            }
        }

        public bool DeleteAudio(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var model = dbContext.Audios.Find(id);
                if (model == null)
                {
                    return false;
                }
                else
                {
                    dbContext.Audios.Remove(model);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<Audio> FindAudio(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.Audios.Where(v => v.Id == id)
                    .Include(x => x.Category)
                    .Include(x => x.CreatedBy)
                    .SingleOrDefaultAsync();
                return entity;
            }
        }

        public async Task<IEnumerable<Audio>> FindAudios(string email, int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn",
            bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Audio> audios;
                if (isDescending)
                {
                    if (orderBy == "Duration")
                    {
                        audios = await dbContext.Audios.Where(v => v.CreatedBy.Email == email)
                                                                .OrderByDescending(x => x.Duration).Skip(skip).Take(pageSize)
                                                                .Include(x => x.CreatedBy)
                                                                .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "UpdatedOn")
                    {
                        audios = await dbContext.Audios.Where(v => v.CreatedBy.Email  == email)
                            .OrderByDescending(x => x.UpdatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "Title")
                    {
                        audios = await dbContext.Audios.Where(v => v.CreatedBy.Email  == email)
                            .OrderByDescending(x => x.Title).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else
                    {
                        audios = await dbContext.Audios.Where(v => v.CreatedBy.Email  == email)
                            .OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }

                }
                else
                {
                    if (orderBy == "Duration")
                    {
                        audios = await dbContext.Audios.Where(v => v.CreatedBy.Email == email)
                            .OrderBy(x => x.Duration).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "UpdatedOn")
                    {
                        audios = await dbContext.Audios.Where(v => v.CreatedBy.Email == email)
                            .OrderBy(x => x.UpdatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "Title")
                    {
                        audios = await dbContext.Audios.Where(v => v.CreatedBy.Email == email)
                            .OrderBy(x => x.Title).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else
                    {
                        audios = await dbContext.Audios.Where(v => v.CreatedBy.Email == email)
                            .OrderBy(x => x.CreatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                }

                return audios;
            };
        }

        public async Task<Audio> UpdateAudio(AudioDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = await dbContext.Audios.Include(v => v.Category).SingleOrDefaultAsync(v => v.Id == dto.Id);
                if (temp == null)
                {
                    throw new AudioNotFoundException(dto.Id);
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

        public async Task<IEnumerable<Audio>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn", bool isDescending = true)
        {
             using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Audio> audios;
                if (isDescending)
                {
                    if (orderBy == "Duration")
                    {
                        audios = await dbContext.Audios.OrderByDescending(x => x.Duration).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "UpdatedOn")
                    {
                        audios = await dbContext.Audios.OrderByDescending(x => x.UpdatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "Title")
                    {
                        audios = await dbContext.Audios.OrderByDescending(x => x.Title).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else
                    {
                        audios = await dbContext.Audios.OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }

                }
                else
                {
                    if (orderBy == "Duration")
                    {
                        audios = await dbContext.Audios.OrderBy(x => x.Duration).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "UpdatedOn")
                    {
                        audios = await dbContext.Audios.OrderBy(x => x.UpdatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else if (orderBy == "Title")
                    {
                        audios = await dbContext.Audios.OrderBy(x => x.Title).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                    else
                    {
                        audios = await dbContext.Audios.OrderBy(x => x.CreatedOn).Skip(skip).Take(pageSize)
                            .Include(x => x.CreatedBy)
                            .Include(x => x.Category).ToListAsync();
                    }
                }

                return audios;
            };
        }


        public async Task<Audio> AddOrUpdateVideo(AudioDto dto)
        {
            if (dto.Id == Guid.Empty)
            {
                return await AddAudio(dto);
            }
            var video = await FindAudio(dto.Id);
            if (video == null)
            {
                return await AddAudio(dto);
            }
            else
            {
                return await UpdateAudio(dto);
            }
        }
    }
}
