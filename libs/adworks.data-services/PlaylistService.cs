using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using adworks.data_services.DbModels;
using adworks.data_services.Identity;
using adworks.media_common;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.data_services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly ILogger _logger;

        public PlaylistService(ILogger logger, IDataContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }


        public async Task<Guid> AddPlaylist(PlaylistDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name))
            {
                throw new InvalidOperationException("playlist name cannot be empty");
            }
            if (dto.StartDate < DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException("Start date must be in the future");
            }
            if (dto.StartDate > dto.EndDate)
            {
                throw new InvalidOperationException("End date must be greater than start date");
            }

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
                var groups = dbContext.DeviceGroups.ToList();
                foreach (var deviceGroup in groups)
                {
                    //filter out unknown device groups
                    //
                    if (dto.DeviceGroups.All(g => g.Name != deviceGroup.Name)
                        || model.PlaylistGroups.Any(g => g.DeviceGroup.Id == deviceGroup.Id))
                    {
                        continue;
                    }
                    var pg = new PlaylistGroup()
                    {
                        DeviceGroup = deviceGroup,
                        Playlist = model,
                        CreatedBy = user
                    };

                    model.PlaylistGroups.Add(pg);
                }

                var result = dbContext.Playlists.Add(model);
                await dbContext.SaveChangesAsync();

                return result.Entity.Id;
            }
        }

        public bool DeletePlaylist(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = dbContext.Playlists.Find(id);
                if (entity == null)
                {
                    return false;
                }
                else
                {
                    dbContext.Playlists.Remove(entity);
                }
                dbContext.SaveChanges();
                return true;
            }
        }

        public async Task<Playlist> FindPlaylist(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.Playlists.Where(x => x.Id == id)
                    .Include(x => x.SubPlaylists).ThenInclude(sp => sp.PlaylistItems)
                    .Include(x => x.PlaylistGroups).ThenInclude(pg => pg.DeviceGroup)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.UpdatedBy)
                    .SingleOrDefaultAsync();
                return entity;
            }
        }

        public async Task<PlaylistItem> FindPlaylistItem(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.PlaylistItems.Where(x => x.Id == id)
                    .Include(x => x.SubPlaylist).ThenInclude(sp => sp.Playlist)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.UpdatedBy)
                    .SingleOrDefaultAsync();
                return entity;
            }
        }

        public async Task<IEnumerable<Playlist>> FindAll(int pageIndex = 0, int pageSize = Int32.MaxValue, string orderBy = "CreatedOn", bool isDescending = true)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                int skip = pageIndex * pageSize;
                IEnumerable<Playlist> playlists;
                if (isDescending)
                {
                    if (orderBy == "Name")
                    {
                        playlists = await dbContext.Playlists
                            .OrderByDescending(x => x.Name).Skip(skip).Take(pageSize).Include(p => p.SubPlaylists)
                            .Include(p => p.PlaylistGroups).ThenInclude(pg => pg.DeviceGroup).ToListAsync();
                    }
                    else
                    {
                        playlists = await dbContext.Playlists
                            .OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize).Include(p => p.SubPlaylists)
                            .Include(p => p.PlaylistGroups).ThenInclude(pg => pg.DeviceGroup).ToListAsync();
                    }
                }
                else
                {
                    if (orderBy == "Name")
                    {
                        playlists = await dbContext.Playlists
                            .OrderByDescending(x => x.Name).Skip(skip).Take(pageSize).Include(p => p.SubPlaylists)
                            .Include(p => p.PlaylistGroups).ThenInclude(pg => pg.DeviceGroup).ToListAsync();
                    }
                    else
                    {
                        playlists = await dbContext.Playlists
                            .OrderByDescending(x => x.CreatedOn).Skip(skip).Take(pageSize).Include(p => p.SubPlaylists)
                            .Include(p => p.PlaylistGroups).ThenInclude(pg => pg.DeviceGroup).ToListAsync();
                    }
                }
                return playlists;
            };
        }

        public async Task<Playlist> UpdatePlaylist(PlaylistDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var temp = dbContext.Playlists.Where(p => p.Id == dto.Id)
                    .Include(p => p.PlaylistGroups)
                    .ThenInclude(pg => pg.DeviceGroup)
                    .Include(p => p.SubPlaylists)
                    .ThenInclude(p => p.PlaylistItems).SingleOrDefault();
                if (temp == null)
                {
                    throw new PlaylistNotFoundException(dto.Id);
                }
                temp.Name = dto.Name;
                temp.StartDate = dto.StartDate;
                temp.EndDate = dto.EndDate;
                temp.UpdatedOn = DateTimeOffset.UtcNow;

                var user = await dbContext.Users.Where(u => u.Email == dto.UpdatedBy).SingleOrDefaultAsync();
                if (user == null)
                {
                    throw new UserNotFoundException(dto.UpdatedBy);
                }
                temp.UpdatedBy = user;

                foreach (var subPlaylistDto in dto.SubPlaylists)
                {
                    if (temp.SubPlaylists.Any(sp => sp.Id != subPlaylistDto.Id))
                    {
                        //create new sub playlist
                        var newSubModel = DtoHelper.Convert(subPlaylistDto);
                        newSubModel.CreatedBy = user;
                        temp.SubPlaylists.Add(newSubModel);
                        subPlaylistDto.PlaylistItems.ToList().ForEach(pi => pi.SubPlaylistId = newSubModel.Id);
                    }

                    foreach (var itemDto in subPlaylistDto.PlaylistItems)
                    {
                        var subPlaylistModel = temp.SubPlaylists.SingleOrDefault(sp => sp.Id == itemDto.SubPlaylistId);
                        var piModel = subPlaylistModel.PlaylistItems.SingleOrDefault(pi =>
                            pi.AssetDiscriminator == itemDto.AssetDiscriminator
                            && pi.MediaAssetId == itemDto.MediaAssetId);
                        if (piModel != null)
                        {
                            // TODO update playlist item properties
                            piModel.MediaAssetId = itemDto.MediaAssetId;
                            piModel.AssetDiscriminator = itemDto.AssetDiscriminator;
                        }
                        else
                        {
                            //create new playlist item
                            var newItemModel = DtoHelper.Convert(itemDto);
                            //validate asset type and id
                            var asset = FindAsset(itemDto.MediaAssetId, itemDto.AssetDiscriminator);
                            if (asset == null)
                            {
                                throw new AssetNotFoundException(itemDto.AssetDiscriminator, itemDto.MediaAssetId);
                            }

                            newItemModel.CreatedBy = user;
                            subPlaylistModel.PlaylistItems.Add(newItemModel);
                        }
                    }
                }

                foreach (var groupDto in dto.DeviceGroups)
                {
                    if (temp.PlaylistGroups.All(pg => pg.DeviceGroup.Id != groupDto.Id))
                    {
                        var group = dbContext.DeviceGroups.Find(groupDto.Id);
                        var model = new PlaylistGroup()
                        {
                            DeviceGroup = group,
                            Playlist = temp,
                            CreatedBy = user
                        };

                        temp.PlaylistGroups.Add(model);
                    }
                }

                var listToRemove = new List<PlaylistGroup>();
                foreach (var pg in temp.PlaylistGroups)
                {
                    if (dto.DeviceGroups.All(dg => dg.Id != pg.DeviceGroup.Id))
                    {
                        listToRemove.Add(pg);
                    }
                }

                foreach (var playlistGroup in listToRemove)
                {
                    temp.PlaylistGroups.Remove(playlistGroup);
                }

                dbContext.Update(temp);
                dbContext.Update(temp);
                await dbContext.SaveChangesAsync();
                return temp;
            }
        }

        public async Task<Guid> AddPlaylistItem(PlaylistItemDto itemDto)
        {
            var itemModel = DtoHelper.Convert(itemDto);
            //validate asset type and id
            var asset = FindAsset(itemDto.MediaAssetId, itemDto.AssetDiscriminator);
            if (asset == null)
            {
                throw new AssetNotFoundException(itemDto.AssetDiscriminator, itemDto.MediaAssetId);
            }

            using (var dbContext = _dbContextFactory.Create())
            {
                var subPlaylist = dbContext.SubPlaylists.Where(sp => sp.Id == itemDto.SubPlaylistId)
                    .Include(sp => sp.Playlist)
                    .SingleOrDefault();

                if (subPlaylist == null)
                {
                    throw new SubPlaylistNotFoundException(itemDto.SubPlaylistId);
                }
                var playlist = subPlaylist.Playlist;

                var user = await dbContext.Users.Where(u => u.Email == itemDto.CreatedBy).SingleOrDefaultAsync();
                if (user == null)
                {
                    throw new UserNotFoundException(itemDto.CreatedBy);
                }
                itemModel.SubPlaylist = subPlaylist;
                itemModel.CreatedBy = user;
                playlist.UpdatedBy = user;
                subPlaylist.UpdatedBy = user;

                dbContext.Update(playlist);
                dbContext.Update(subPlaylist);
                dbContext.PlaylistItems.Add(itemModel);
                await dbContext.SaveChangesAsync();
                return itemModel.Id;
            }
        }

        public MediaAsset FindAsset(Guid assetId, string assetType)
        {
            MediaAsset asset = null;
            using (var dbContext = _dbContextFactory.Create())
            {
                if (assetType == AssetTypes.Video)
                {
                    asset = dbContext.Videos.Find(assetId);
                }
                else if (assetType == AssetTypes.Audio)
                {
                    asset = dbContext.Audios.Find(assetId);
                }
                else if (assetType == AssetTypes.Image)
                {
                    asset = dbContext.Images.Find(assetId);
                }
                else
                {
                    throw new InvalidAssetTypeException(assetType);
                }
            }
            return asset;
        }

        public async Task<Playlist> RemovePlaylistItem(RemovePlaylistItemDto dto)
        {
            Guid playlistId = dto.PlaylistId;
            Guid playlistItemId = dto.PlaylistItemId;
            Guid subPlaylistId = dto.SubPlaylistId;
            using (var dbContext = _dbContextFactory.Create())
            {
                var playlist = await dbContext.Playlists.Where(p => p.Id == playlistId)
                    .Include(p => p.SubPlaylists)
                    .ThenInclude(sp => sp.PlaylistItems)
                    .SingleOrDefaultAsync();

                var user = await dbContext.Users.Where(u => u.Email == dto.UpdatedBy).SingleOrDefaultAsync();
                if (user == null)
                {
                    throw new UserNotFoundException(dto.UpdatedBy);
                }
                playlist.UpdatedBy = user;

                var subPlaylist = playlist.SubPlaylists.SingleOrDefault(p => p.Id == subPlaylistId);
                if (subPlaylist == null)
                {
                    throw new SubPlaylistNotFoundException(subPlaylistId);
                }

                var item = subPlaylist.PlaylistItems.SingleOrDefault(p => p.Id == playlistItemId);
                if (item == null)
                {
                    throw new PlaylistItemNotFoundException(subPlaylistId);
                }

                subPlaylist.PlaylistItems.Remove(item);

                dbContext.Update(playlist);
                dbContext.Update(subPlaylist);
                dbContext.PlaylistItems.Remove(item);
                await dbContext.SaveChangesAsync();
                return playlist;
            }
        }

        private void UpdatePlaylistItemFields(PlaylistItem model, PlaylistItemDto dto, User user = null)
        {
            model.Index = dto.Index;
            model.Duration = dto.Duration;
            model.MediaAssetId = dto.MediaAssetId;
            model.AssetDiscriminator = dto.AssetDiscriminator;
            model.Duration = dto.Duration;
            model.UpdatedOn = DateTimeOffset.UtcNow;
            model.UpdatedBy = user;
        }

        public async Task<PlaylistItem> UpdatePlaylistItem(PlaylistItemDto dto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var item = await dbContext.PlaylistItems.SingleOrDefaultAsync(pi => pi.MediaAssetId == dto.MediaAssetId
                                                                                    && pi.AssetDiscriminator ==
                                                                                    dto.AssetDiscriminator);
                var user = await dbContext.Users.Where(u => u.Email == dto.UpdatedBy).SingleOrDefaultAsync();
                if (user == null)
                {
                    throw new UserNotFoundException(dto.UpdatedBy);
                }

                UpdatePlaylistItemFields(item, dto, user);

                await dbContext.SaveChangesAsync();
                return item;
            }
        }

        public async Task<Playlist> Validate(Guid id)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var entity = await dbContext.Playlists.Where(x => x.Id == id)
                    .Include(x => x.SubPlaylists).ThenInclude(x => x.PlaylistItems)
                    .Include(x => x.PlaylistGroups).ThenInclude(pg => pg.DeviceGroup)
                    .SingleOrDefaultAsync();
                var count = entity.SubPlaylists.Select(s => s.PlaylistItems).Aggregate(0, (acc, x) => acc + x.Count);

                if (count == 0)
                {
                    throw new ValidationException("Playlist is empty");
                }
                if (entity.PlaylistGroups.Count == 0)
                {
                    throw new ValidationException("You must specify at least one group to publish");
                }
                if (entity.StartDate > entity.EndDate)
                {
                    throw new ValidationException("End date must be equal or greater than start date");
                }
                if (entity.EndDate < DateTimeOffset.UtcNow)
                {
                    throw new ValidationException("End date must be in the future");
                }

                return entity;
            }
        }

        public async Task<Guid> AddSubPlaylist(SubPlaylistDto subDto)
        {
            using (var dbContext = _dbContextFactory.Create())
            {
                var model = DtoHelper.Convert(subDto);

                var playlist = await dbContext.Playlists.FindAsync(subDto.PlaylistId);
                if (playlist == null)
                {
                    throw new PlaylistNotFoundException(subDto.PlaylistId);
                }
                model.Playlist = playlist;
                var user = await dbContext.Users.Where(u => u.Email == subDto.CreatedBy).SingleOrDefaultAsync();
                if (user == null)
                {
                    throw new UserNotFoundException(subDto.CreatedBy);
                }
                model.CreatedBy = user;
                dbContext.SubPlaylists.Add(model);

                await dbContext.SaveChangesAsync();
                return model.Id;
            }
        }
    }
}
