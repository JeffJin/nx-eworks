using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.media_common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player
{
    public class DataService: IDataService
    {
        private readonly ILogger _logger;

        public DataService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task Init()
        {
            _logger.Information("Database is being initialized");

            using (var context = new PiDbContext())
            {
                var dbCreator = context.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                if(dbCreator != null && dbCreator.Exists())
                {
                    return;
                }
                await context.Database.EnsureCreatedAsync();
                await context.SaveChangesAsync();
            }

            _logger.Information("Database initialized");
        }

        public async Task<IEnumerable<Record>> GetRecords()
        {
            using (var dbContext = new PiDbContext())
            {
                return await dbContext.Records.ToListAsync();
            }
        }

        public async Task<Record> AddRecord(Record record)
        {
            using (var dbContext = new PiDbContext())
            {
                var result = await dbContext.Records.AddAsync(record);
                await dbContext.SaveChangesAsync();
                return result.Entity;
            }
        }

        public async Task RemoveRecord(Guid id)
        {
            using (var dbContext = new PiDbContext())
            {
                var result = await dbContext.Records.FindAsync(id);
                if (result == null)
                {
                    return;
                }

                dbContext.Records.Remove(result);
                await dbContext.SaveChangesAsync();
            }
        }

        public PlaylistDto GetPlaylist(Guid id)
        {
            using (var dbContext = new PiDbContext())
            {
                var result = dbContext.Playlists.Find(id);
                if (result == null)
                {
                    return null;
                }

                PlaylistDto dto = SerializeHelper.Deserialize<PlaylistDto>(result.Content);

                return dto;
            }
        }

        public void RemovePlaylist(Guid id)
        {
            using (var dbContext = new PiDbContext())
            {
                var result = dbContext.Playlists.Find(id);
                if (result == null)
                {
                    return;
                }
                dbContext.Playlists.Remove(result);
                dbContext.SaveChanges();
            }
        }

        public async Task SavePlaylist(PlaylistDto newPlaylist)
        {
            _logger.Information($"Save playlist {newPlaylist.Name} to database");

            using (var dbContext = new PiDbContext())
            {
                string newContent = SerializeHelper.Stringify(newPlaylist);
                var result = await dbContext.Playlists.FindAsync(newPlaylist.Id);
                if (result == null)
                {
                    var playlist = new Playlist()
                    {
                        Id = newPlaylist.Id,
                        Content = newContent
                    };
                    dbContext.Playlists.Add(playlist);
                }
                else
                {
                    result.Content = newContent;
                }

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<DeviceDto> GetDevice()
        {
            using (var dbContext = new PiDbContext())
            {
                var result = await dbContext.Devices.OrderByDescending(d => d.CreatedOn).FirstOrDefaultAsync();
                if(result == null)
                {
                    return null;
                }
                DeviceDto dto = SerializeHelper.Deserialize<DeviceDto>(result.DeviceInfo);
                return dto;
            }
        }

        public void RemoveDevice(Guid id)
        {
            using (var dbContext = new PiDbContext())
            {
                var result = dbContext.Devices.Find(id);
                if (result == null)
                {
                    return;
                }
                dbContext.Devices.Remove(result);
                dbContext.SaveChanges();
            }
        }

        public async Task RemoveAll()
        {
            using (var dbContext = new PiDbContext())
            {
                var devices = dbContext.Devices.AsEnumerable();
                dbContext.Devices.RemoveRange(devices);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task SaveDevice(DeviceDto deviceDto)
        {
            using (var dbContext = new PiDbContext())
            {
                string newDeviceInfo = SerializeHelper.Stringify(deviceDto);
                var result = await dbContext.Devices.FindAsync(deviceDto.Id);
                if (result == null)
                {
                    var model = new Device()
                    {
                        Id = deviceDto.Id,
                        DeviceInfo = newDeviceInfo
                    };
                    dbContext.Devices.Add(model);
                }
                else
                {
                    result.DeviceInfo = newDeviceInfo;
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
