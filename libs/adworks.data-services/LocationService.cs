using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.media_common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class LocationService : ILocationService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public LocationService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Guid> AddLocation(LocationDto dto)
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
                var result = dbContext.Locations.Add(model);
                await dbContext.SaveChangesAsync();

                return result.Entity.Id;
            }
        }

        public bool DeleteLocation(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = dbContext.Locations.Find(id);
                if (entity == null)
                {
                    return false;
                }
                else
                {
                    dbContext.Locations.Remove(entity);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<Location> FindLocation(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.Locations.Where(v => v.Id == id)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Devices)
                    .SingleOrDefaultAsync();
                return entity;
            }
        }

        public async Task<IEnumerable<Location>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn", bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Location> locations;
                if (isDescending)
                {
                    if (orderBy == "Address")
                    {
                        locations = await dbContext.Locations
                            .OrderByDescending(x => x.Address).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else if (orderBy == "Locale")
                    {
                        locations = await dbContext.Locations
                            .OrderByDescending(x => x.Locale).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else
                    {
                        locations = await dbContext.Locations
                            .OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize).ToListAsync();
                    }
                }
                else
                {
                    if (orderBy == "Address")
                    {
                        locations = await dbContext.Locations
                            .OrderBy(x => x.Address).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else if (orderBy == "Locale")
                    {
                        locations = await dbContext.Locations
                            .OrderBy(x => x.Locale).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else
                    {
                        locations = await dbContext.Locations
                            .OrderBy(x => x.CreatedOn).Skip(skip).Take(pageSize).ToListAsync();
                    }
                }
                return locations;
            };
        }

        public async Task<Location> UpdateLocation(LocationDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = await dbContext.Locations.FindAsync(dto.Id);
                if (temp == null)
                {
                    throw new DeviceNotFoundException(dto.Id);
                }
                temp.Address = dto.Address;
                temp.Locale = dto.Locale;
                temp.TimezoneOffset = dto.TimezoneOffset;
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
    }
}
