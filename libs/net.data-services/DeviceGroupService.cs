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
    public class DeviceGroupService : IDeviceGroupService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public DeviceGroupService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _logger = logger;

            _dbContextFactory = dbContextFactory;
        }

        public async Task<Guid> AddGroup(DeviceGroupDto dto)
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
                var organization = await dbContext.Organizations.Where(u => u.Name == dto.OrganizationName).SingleOrDefaultAsync();
                if (organization != null)
                {
                    model.Organization = organization;
                }
                var result = dbContext.DeviceGroups.Add(model);
                await dbContext.SaveChangesAsync();

                return result.Entity.Id;
            }
        }

        public bool DeleteDeviceGroup(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = dbContext.DeviceGroups.Find(id);
                if (entity == null)
                {
                    return false;
                }
                else
                {
                    dbContext.DeviceGroups.Remove(entity);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<DeviceGroup> FindGroup(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.DeviceGroups.Where(v => v.Id == id)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Organization)
                    .Include(x => x.PlaylistGroups)
                    .Include(x => x.Devices)
                    .SingleOrDefaultAsync();
                return entity;
            }
        }

        public async Task<IEnumerable<DeviceGroupDto>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn", bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IQueryable<DeviceGroup> results;
                if (isDescending)
                {
                    if (orderBy == "Name")
                    {
                        results = dbContext.DeviceGroups.OrderByDescending(x => x.Name).Skip(skip).Take(pageSize);
                    }
                    else
                    {
                        results = dbContext.DeviceGroups.OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize);
                    }
                }
                else
                {
                    if (orderBy == "Name")
                    {
                        results = dbContext.DeviceGroups.OrderBy(x => x.Name).Skip(skip).Take(pageSize);
                    }
                    else
                    {
                        results = dbContext.DeviceGroups.OrderBy(x => x.CreatedOn).Skip(skip).Take(pageSize);
                    }
                }

                var temp = from model in results
                    select new DeviceGroupDto()
                    {
                        Id = model.Id,
                        CreatedBy = model.CreatedBy.Email,
                        CreatedOn = model.CreatedOn,
                        Name = model.Name,
                        OrganizationName = model.Organization.Name,
                        NumOfDevices = model.Devices.Count,
                        NumOfPlaylists = model.PlaylistGroups.Count
                    };

                return await temp.ToListAsync();
            };
        }

        public async Task<DeviceGroup> UpdateDeviceGroup(DeviceGroupDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = await dbContext.DeviceGroups.FindAsync(dto.Id);
                if (temp == null)
                {
                    throw new DeviceGroupNotFoundException(dto.Id);
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

                var organization = await dbContext.Organizations.Where(u => u.Name == dto.OrganizationName).SingleOrDefaultAsync();
                if (organization != null)
                {
                    temp.Organization = organization;
                }

                await dbContext.SaveChangesAsync();
                return temp;
            }
        }

        public async Task<Guid> AddDeviceToGroup(DeviceDto deviceDto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var deviceGroup = await dbContext.DeviceGroups.Where(v => v.Name == deviceDto.DeviceGroupName)
                    .SingleOrDefaultAsync();

                var device = dbContext.Devices.SingleOrDefault(v => v.Id == deviceDto.Id);

                if (device == null)
                {
                    var deviceModel = DtoHelper.Convert(deviceDto);
                    deviceModel.CreatedBy = dbContext.Users.Single(u => u.Email == deviceDto.CreatedBy);
                    deviceModel.DeviceGroup = deviceGroup;
                    device = dbContext.Devices.Add(deviceModel).Entity;
                }

                if (deviceDto.LocationId != null)
                {
                    var location = dbContext.Locations.Find(deviceDto.LocationId);
                    if (location != null)
                    {
                        device.Location = location;
                    }
                }

                await dbContext.SaveChangesAsync();
                return device.Id;
            }
        }

        public async Task<PlaylistGroup> AddPlaylistGroup(PlaylistGroupDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var deviceGroup = await dbContext.DeviceGroups.FindAsync(dto.DeviceGroupId);
                if (deviceGroup == null)
                {
                    throw new DeviceGroupNotFoundException(dto.DeviceGroupId);
                }
                var playlist =  await dbContext.Playlists.FindAsync(dto.PlaylistId);
                if (playlist == null)
                {
                    throw new PlaylistNotFoundException(dto.PlaylistId);
                }

                var model = new PlaylistGroup()
                {
                    Playlist = playlist,
                    DeviceGroup = deviceGroup,
                };
                var user = await dbContext.Users.Where(u => u.Email == dto.CreatedBy).SingleOrDefaultAsync();
                if (user != null)
                {
                    deviceGroup.UpdatedBy = user;
                    playlist.UpdatedBy = user;
                    model.CreatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(dto.CreatedBy);
                }

                dbContext.PlaylistGroups.Add(model);
                await dbContext.SaveChangesAsync();
                return model;
            }
        }

        public async Task<PlaylistGroup> FindPlaylistGroup(Guid playlistGroupId)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var item = await dbContext.PlaylistGroups.Where(p => p.Id == playlistGroupId)
                    .Include(p => p.Playlist)
                    .Include(p => p.DeviceGroup)
                    .SingleOrDefaultAsync();
                return item;
            }
        }

        public async Task<bool> RemovePlaylistGroup(PlaylistGroupDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var deviceGroup = await dbContext.DeviceGroups.FindAsync(dto.DeviceGroupId);
                if (deviceGroup == null)
                {
                    throw new DeviceGroupNotFoundException(dto.DeviceGroupId);
                }
                var playlist =  await dbContext.Playlists.FindAsync(dto.PlaylistId);
                if (playlist == null)
                {
                    throw new PlaylistNotFoundException(dto.PlaylistId);
                }

                var model = await dbContext.PlaylistGroups
                    .Include(d => d.Playlist)
                    .Include(d=> d.DeviceGroup)
                    .SingleAsync(m =>
                    m.DeviceGroup.Id == dto.DeviceGroupId && m.Playlist.Id == dto.PlaylistId);

                var user = await dbContext.Users.Where(u => u.Email == dto.CreatedBy).SingleOrDefaultAsync();
                if (user != null)
                {
                    deviceGroup.UpdatedBy = user;
                    playlist.UpdatedBy = user;
                }
                else
                {
                    throw new UserNotFoundException(dto.CreatedBy);
                }

                if (model == null)
                {
                    return false;
                }

                deviceGroup.PlaylistGroups.Remove(model);
                await dbContext.SaveChangesAsync();
                return true;
            }
        }

        public async Task<IEnumerable<DeviceGroupDto>> GetGroupsWithDevices(int number = 4)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = from dg in dbContext.DeviceGroups
                    select new
                    {
                        Group = dg,
                        NumOfDevices = dg.Devices.Count
                    };

                var groups = await temp.OrderByDescending(t => t.NumOfDevices).Take(number).Select(a => a.Group).ToListAsync();

                var groupDtos = groups.Select(DtoHelper.Convert);

                var groupsWithDevices = groupDtos as DeviceGroupDto[] ?? groupDtos.ToArray();
                foreach (var deviceGroup in groups)
                {
                    var groupDto = groupsWithDevices.Single(gd => gd.Id == deviceGroup.Id);
                    //TODO test
                    var devices = dbContext.Devices
                        .Include(d => d.DeviceGroup)
                        .Where(d => d.DeviceGroup.Id == deviceGroup.Id)
                        .ToList();
                    foreach (var device in devices)
                    {
                        var deviceDto = DtoHelper.Convert(device);
                        var lastStatus = await dbContext.DeviceStatuses
                            .Include(ds=>ds.Device)
                            .Where(x => x.Device.Id == device.Id)
                            .OrderByDescending(p => p.CreatedOn)
                            .FirstOrDefaultAsync();
                        deviceDto.LastStatus = DtoHelper.Convert(lastStatus);

                        var licenses = await dbContext.Licenses
                            .Include(l => l.Device)
                            .Where(x => x.Device.Id == device.Id)
                            .ToListAsync();
                        foreach (var license in licenses)
                        {
                            deviceDto.Licenses.Add(DtoHelper.Convert(license));
                        }
                        groupDto.Devices.Add(deviceDto);
                    }

                    groupDto.NumOfDevices = devices.Count();
                }

                return groupsWithDevices;

            };
        }
    }
}
