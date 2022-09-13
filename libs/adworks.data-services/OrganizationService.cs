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
    public class OrganizationService : IOrganizationService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public OrganizationService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Guid> AddOrganization(OrganizationDto dto)
        {
            using (var context = _dbContextFactory.Create())
            {
                var model = DtoHelper.Convert(dto);
                var user = await context.Users.Where(u => u.Email == dto.CreatedBy).SingleOrDefaultAsync();
                if (user != null)
                {
                    model.CreatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(dto.CreatedBy);
                }
                var result = await context.Organizations.AddAsync(model);
                context.SaveChanges();
                return result.Entity.Id;
            }
        }

        public bool DeleteOrganization(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = dbContext.Organizations.Find(id);
                if (entity == null)
                {
                    return false;
                }
                else
                {
                    dbContext.Organizations.Remove(entity);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<Organization> FindOrganization(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.Organizations.Where(v => v.Id == id)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Users)
                    .Include(x => x.DeviceGroups)
                    .SingleOrDefaultAsync();
                return entity;
            }
        }

        public async Task<Organization> FindOrganizationByName(string name)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.Organizations.FirstOrDefaultAsync(v => v.Name == name);
                return entity;
            }
        }

        public async Task<int> AddUser(string orgName, string email)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var org = await dbContext.Organizations.FirstOrDefaultAsync(v => v.Name == orgName);
                var user = await dbContext.Users.FirstOrDefaultAsync(v => v.Email == email);
                org.Users.Add(user);
                var result = await dbContext.SaveChangesAsync();
                return result;
            }
        }

        public async Task<IEnumerable<Organization>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn", bool isDescending = true)
        {
           using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Organization> results;
                if (isDescending)
                {
                    if (orderBy == "Name")
                    {
                        results = await dbContext.Organizations.OrderByDescending(x => x.Name).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else
                    {
                        results = await dbContext.Organizations.OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize).ToListAsync();
                    }
                }
                else
                {
                    if (orderBy == "Name")
                    {
                        results = await dbContext.Organizations.OrderBy(x => x.Name).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else
                    {
                        results = await dbContext.Organizations.OrderBy(x => x.CreatedOn).Skip(skip).Take(pageSize).ToListAsync();
                    }
                }

                return results;
            };
        }

        public async Task<Organization> UpdateOrganization(OrganizationDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = await dbContext.Organizations.FindAsync(dto.Id);
                if (temp == null)
                {
                    throw new OrganizationNotFoundException(dto.Id);
                }
                temp.Name = dto.Name;
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
