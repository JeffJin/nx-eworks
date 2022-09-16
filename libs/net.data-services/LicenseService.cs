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
    public class LicenseService : ILicenseService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public LicenseService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Guid> AddLicense(LicenseDto dto)
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

                if (dto.DeviceId != null)
                {
                    var device = await dbContext.Devices.FindAsync(dto.DeviceId);
                    if (device != null)
                    {
                        model.Device = device;
                    }
                }

                var result = dbContext.Licenses.Add(model);
                await dbContext.SaveChangesAsync();

                return result.Entity.Id;
            }
        }

        public async Task<IList<License>> GetLicenses()
        {
            using (var dbContext = _dbContextFactory.Create())
            {

                var result = await dbContext.Licenses.ToListAsync();

                return result;
            }
        }

        public License CreateTrialLicense(int months = 3)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var user = dbContext.Users.SingleOrDefault(u => u.Email == "admin@eworkspace.ca");

                var trial = new License()
                {
                    Type = "Trial",
                    ExpireOn = DateTimeOffset.UtcNow.AddMonths(months),
                    CreatedBy = user
                };

                var result = dbContext.Licenses.Add(trial);
                dbContext.SaveChanges();
                return result.Entity;
            }
        }

        public async Task<IList<License>> FindLicenses(IEnumerable<Guid> licenseIds)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entities = await dbContext.Licenses.Where(v => licenseIds.Contains(v.Id)).ToListAsync();
                return entities;
            }
        }

        public bool DeleteLicense(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = dbContext.Licenses.Find(id);
                if (entity == null)
                {
                    return false;
                }
                else
                {
                    dbContext.Licenses.Remove(entity);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<License> FindLicense(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.Licenses.Where(v => v.Id == id)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Device)
                    .SingleOrDefaultAsync();
                return entity;
            }
        }

        public async Task<License> UpdateLicense(LicenseDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = await dbContext.Licenses.FindAsync(dto.Id);
                if (temp == null)
                {
                    throw new DeviceNotFoundException(dto.Id);
                }
                temp.ExpireOn = dto.ExpireOn;
                temp.Type = dto.Type;
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
