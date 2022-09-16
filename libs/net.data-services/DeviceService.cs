using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.data_services.Identity;
using adworks.media_common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public DeviceService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<Guid> RegisterDevice(DeviceDto dto)
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

                if (!string.IsNullOrEmpty(dto.DeviceGroupName))
                {
                    var group = dbContext.DeviceGroups.SingleOrDefault(v => v.Name == dto.DeviceGroupName);
                    if (group != null)
                    {
                        model.DeviceGroup = group;
                    }
                }

                if (dto.LocationId != null)
                {
                    var location = dbContext.Locations.Find(dto.LocationId);
                    if (location != null)
                    {
                        model.Location = location;
                    }
                }

                if (dto.Licenses != null && dto.Licenses.Count > 0)
                {
                    foreach (var license in dto.Licenses)
                    {
                        var licenceFound = dbContext.Licenses.Find(license.Id);
                        if (licenceFound != null)
                        {
                            model.Licenses.Add(licenceFound);
                            licenceFound.Device = model;
                            dbContext.Update(licenceFound);
                        }
                    }
                }

                var result = dbContext.Devices.Add(model);

                await dbContext.SaveChangesAsync();

                return result.Entity.Id;
            }
        }

        public bool DeleteDevice(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = dbContext.Devices.Find(id);
                if (entity == null)
                {
                    return false;
                }
                else
                {
                    dbContext.Devices.Remove(entity);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<Device> FindDevice(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.Devices.Where(v => v.Id == id)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Location)
                    .Include(x => x.Licenses)
                    .Include(x => x.DeviceGroup)
                    .SingleOrDefaultAsync();
                return entity;
            }
        }

        public async Task<IEnumerable<Device>> FindDevicesByGroup(string groupName, int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn", bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Device> devices;
                if (isDescending)
                {
                    if (orderBy == "AssetTag")
                    {
                        devices = await dbContext.Devices.Include(x => x.DeviceGroup).Where(v => v.DeviceGroup.Name == groupName)
                            .OrderByDescending(x => x.AssetTag).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else if (orderBy == "GroupName")
                    {
                        devices = await dbContext.Devices.Include(x => x.DeviceGroup).Where(v => v.DeviceGroup.Name == groupName)
                            .OrderByDescending(x => x.DeviceGroup.Name).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else
                    {
                        devices = await dbContext.Devices.Include(x => x.DeviceGroup).Where(v => v.DeviceGroup.Name == groupName)
                            .OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize).ToListAsync();
                    }
                }
                else
                {
                    if (orderBy == "AssetTag")
                    {
                        devices = await dbContext.Devices.Include(x => x.DeviceGroup).Where(v => v.DeviceGroup.Name == groupName)
                            .OrderBy(x => x.AssetTag).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else if (orderBy == "GroupName")
                    {
                        devices = await dbContext.Devices.Include(x => x.DeviceGroup).Where(v => v.DeviceGroup.Name == groupName)
                            .OrderBy(x => x.DeviceGroup.Name).Skip(skip).Take(pageSize).ToListAsync();
                    }
                    else
                    {
                        devices = await dbContext.Devices.Include(x => x.DeviceGroup).Where(v => v.DeviceGroup.Name == groupName)
                            .OrderBy(x => x.CreatedOn).Skip(skip).Take(pageSize).ToListAsync();
                    }
                }

                return devices;
            };
        }

        public async Task<IEnumerable<Device>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn", bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Device> devices;
                if (isDescending)
                {
                    if (orderBy == "AssetTag")
                    {
                        devices = await dbContext.Devices
                            .OrderByDescending(x => x.AssetTag).Skip(skip).Take(pageSize).Include(d => d.DeviceGroup).ToListAsync();
                    }
                    else if (orderBy == "GroupName")
                    {
                        devices = await dbContext.Devices
                            .OrderByDescending(x => x.DeviceGroup.Name).Skip(skip).Take(pageSize).Include(d => d.DeviceGroup).ToListAsync();
                    }
                    else
                    {
                        devices = await dbContext.Devices
                            .OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize).Include(d => d.DeviceGroup).ToListAsync();
                    }

                }
                else
                {
                    if (orderBy == "AssetTag")
                    {
                        devices = await dbContext.Devices
                            .OrderBy(x => x.AssetTag).Skip(skip).Take(pageSize).Include(d => d.DeviceGroup).ToListAsync();
                    }
                    else if (orderBy == "GroupName")
                    {
                        devices = await dbContext.Devices
                            .OrderBy(x => x.DeviceGroup.Name).Skip(skip).Take(pageSize).Include(d => d.DeviceGroup).ToListAsync();
                    }
                    else
                    {
                        devices = await dbContext.Devices
                            .OrderBy(x => x.CreatedOn).Skip(skip).Take(pageSize).Include(d => d.DeviceGroup).ToListAsync();
                    }
                }
                return devices;
            };
        }

        public async Task<Device> UpdateDevice(DeviceDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var model = await dbContext.Devices.FindAsync(dto.Id);
                if (model == null)
                {
                    throw new DeviceNotFoundException(dto.Id);
                }
                model.SerialNumber = dto.SerialNumber;
                model.AppVersion = dto.AppVersion;
                model.AssetTag = dto.AssetTag;
                model.DeviceVersion = dto.DeviceVersion;
                model.UpdatedOn = DateTimeOffset.UtcNow;
                if (!string.IsNullOrEmpty(dto.UpdatedBy))
                {
                    var user = await dbContext.Users.Where(u => u.Email == dto.UpdatedBy).SingleOrDefaultAsync();
                    if (user != null)
                    {
                        model.UpdatedBy = user;
                    }
                    else
                    {
                        throw new UserNotFoundException(dto.UpdatedBy);
                    }
                }
                if (dto.LocationId != null)
                {
                    model.Location = dbContext.Locations.Find(dto.LocationId);
                }
                else
                {
                    model.Location = null;
                }

                if (!string.IsNullOrEmpty(dto.DeviceGroupName))
                {
                    model.DeviceGroup = dbContext.DeviceGroups.SingleOrDefault(v => v.Name == dto.DeviceGroupName);
                }
                else
                {
                    model.DeviceGroup = null;
                }

                await dbContext.SaveChangesAsync();
                return model;
            }
        }

        public async Task<DeviceStatus> GetDeviceStatus(string serialNumber)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var device = await dbContext.Devices.SingleAsync(d => d.SerialNumber == serialNumber);
                var model = await dbContext.DeviceStatuses
                    .Include(ds => ds.Device)
                    .Where(x => x.Device.Id == device.Id)
                    .OrderByDescending(p => p.CreatedOn)
                    .FirstOrDefaultAsync();
                return model;
            }
        }

        public async Task<DeviceStatus> AddOrUpdateDeviceStatus(DeviceStatusDto statusDto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var model = DtoHelper.Convert(statusDto);

                var device = await dbContext.Devices.SingleOrDefaultAsync(d => d.SerialNumber == statusDto.SerialNumber);
                if (device != null)
                {
                    model.Device = device;
                }
                else
                {
                    throw new DeviceNotFoundException(statusDto.SerialNumber);
                }
                var user = await dbContext.Users.Where(u => u.Email == statusDto.CreatedBy).SingleOrDefaultAsync();
                if (user != null)
                {
                    model.CreatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(statusDto.CreatedBy);
                }

                var statuses = dbContext.DeviceStatuses
                    .Include(ds => ds.Device)
                    .Where(ds => ds.Device.Id == device.Id)
                    .OrderBy(ds => ds.CreatedOn);
                DeviceStatus result = null;
                if (statuses.Count() > 1)
                {
                    //this is exception case, there should always be one latest record.
                    //in this case, remove all old records and add a new one
                    dbContext.DeviceStatuses.RemoveRange(statuses);
                    result = (await dbContext.DeviceStatuses.AddAsync(model)).Entity;
                }
                else if (statuses.Count() == 1)
                {
                    //update time stamp only
                    var status = statuses.Single();
                    status.UpdatedOn = model.UpdatedOn;
                    result = status;
                }
                else
                {
                    result = (await dbContext.DeviceStatuses.AddAsync(model)).Entity;
                }

                await dbContext.SaveChangesAsync();
                return result;
            }
        }

        public async Task<bool> CheckLicense(string serialNumber)
        {
            using (var dbContext = _dbContextFactory.Create())
            {

                var device = await dbContext.Devices.Where(d => String.Compare(d.SerialNumber, serialNumber, StringComparison.OrdinalIgnoreCase) == 0).Include(d => d.Licenses).SingleOrDefaultAsync();
                if (device == null)
                {
                    throw new DeviceNotFoundException(serialNumber);
                }

                return device.Licenses.Any(l => l.ExpireOn > DateTimeOffset.UtcNow);
            }
        }

        public async Task<int> RemoveDeviceStatus(Guid deviceId, int offsetDays)
        {
            if (offsetDays > 0)
            {
                throw new InvalidDataException("Offset days value must be less than 0");
            }

            await using (var dbContext = _dbContextFactory.Create())
            {
                var expectedDate = DateTimeOffset.UtcNow.AddDays(offsetDays);
                var results = from ds in dbContext.DeviceStatuses.Include(ds => ds.Device)
                    where ds.Device.Id == deviceId && ds.CreatedOn <= expectedDate
                    select ds;

                if (!results.Any())
                {
                    return 0;
                }

                dbContext.DeviceStatuses.RemoveRange(results);
                await dbContext.SaveChangesAsync();
                return results.Count();
            }
        }

        public async Task<Device> FindDeviceBySerialNumber(string serialNumber)
        {
            await using var dbContext = _dbContextFactory.Create();
            var entity = await dbContext.Devices.Where(d => d.SerialNumber == serialNumber)
                .Include(x => x.CreatedBy)
                .Include(x => x.Location)
                .Include(x => x.Licenses)
                .Include(x => x.DeviceGroup).ThenInclude(g => g.Organization)
                .SingleOrDefaultAsync();
            return entity;
        }

        public async Task<IList<User>> GetAssociatedUsers(string serialNumber)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var device = await dbContext.Devices.Where(d => d.SerialNumber == serialNumber)
                    .Include(x => x.DeviceGroup).ThenInclude(g => g.Organization).ThenInclude(c => c.Users).SingleOrDefaultAsync();
                return device?.DeviceGroup?.Organization?.Users.ToList();
            }
        }
    }
}
