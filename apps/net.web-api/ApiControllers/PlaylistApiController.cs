using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;
using adworks.media_web_api.Middlewares;
using adworks.message_bus;
using adworks.message_common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/playlists")]
    public class PlaylistApiController : BaseApiController
    {
        private readonly IPlaylistService _playlistService;
        private readonly IDeviceService _deviceService;
        private readonly IMessageClient _messageClient;

        // Get the default form options so that we can use them to set the default limits for
        // request body data

        public PlaylistApiController(IPlaylistService playlistService, IDeviceService deviceService,
            IMessageClient messageClient, IConfiguration configuration,
            IUserService userService, ILogger logger): base(configuration, userService, logger)
        {
            _playlistService = playlistService;
            _deviceService = deviceService;
            _messageClient = messageClient;
        }

        // GET list
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<PlaylistDto>> Get()
        {
            //TODO get user context and filter by user email

            IEnumerable<Playlist> models = await _playlistService.FindAll();
            return models.Select(m =>
            {
                var dto = DtoHelper.Convert(m);
                foreach (var playlistGroup in m.PlaylistGroups)
                {
                    // TODO playlistGroup.DeviceGroup.Id
                    dto.DeviceGroups.Add(new DeviceGroupDto(){Id = playlistGroup.DeviceGroup.Id});
                }

                foreach (var subList in m.SubPlaylists)
                {
                    var subPlDto = DtoHelper.Convert(subList);
                    dto.SubPlaylists.Add(subPlDto);
                }

                return dto;
            });
        }

        // GET details/id
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<PlaylistDto> GetDetails(Guid id)
        {
            var model = await _playlistService.FindPlaylist(id);

            var dto = DtoHelper.Convert(model);
            foreach (var playlistGroup in model.PlaylistGroups)
            {
                dto.DeviceGroups.Add(DtoHelper.Convert(playlistGroup.DeviceGroup));
            }
            foreach (var sub in model.SubPlaylists)
            {
                var subDto = DtoHelper.Convert(sub);
                dto.SubPlaylists.Add(subDto);

                foreach (var playlistItem in sub.PlaylistItems)
                {
                    subDto.PlaylistItems.Add(DtoHelper.Convert(playlistItem));
                }
            }
            return dto;

        }

        // POST create
        [HttpPost]
        public async Task<Guid> Create([FromBody] PlaylistDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name.Trim()))
            {
                throw new InvalidRequstException("Playlist name cannot be null or empty");
            }

            dto.CreatedBy = GetCurrentEmail();
            dto.Id = Guid.NewGuid();
            dto.Name = dto.Name.Trim();

            dto.CreatedBy = GetCurrentEmail();
            return await _playlistService.AddPlaylist(dto);
        }

        // PUT update
        [HttpPut("update")]
        public async Task<PlaylistDto> Update([FromBody] PlaylistDto dto)
        {
            try
            {
                dto.UpdatedBy = GetCurrentEmail();
                var result = await _playlistService.UpdatePlaylist(dto);
                var resultDto = DtoHelper.Convert(result);
                foreach (var playlistGroup in result.PlaylistGroups)
                {
                    resultDto.DeviceGroups.Add(DtoHelper.Convert(playlistGroup.DeviceGroup));
                }

                foreach (var sub in result.SubPlaylists)
                {
                    var subDto = DtoHelper.Convert(sub);
                    resultDto.SubPlaylists.Add(subDto);

                    foreach (var playlistItem in sub.PlaylistItems)
                    {
                        subDto.PlaylistItems.Add(DtoHelper.Convert(playlistItem));
                    }
                }

                return resultDto;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update playlist", ex);
                throw;
            }
        }


        // Put addPlaylistItem
        [HttpPut("addPlaylistItem")]
        public async Task<Guid> AddPlaylistItem([FromBody] PlaylistItemDto dto)
        {
            try
            {
                dto.CreatedBy = GetCurrentEmail();
                var result = await _playlistService.AddPlaylistItem(dto);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to add playlist item", ex);
                throw;
            }
        }

        // Put addPlaylistItem
        [HttpPut("updatePlaylistItem")]
        public async Task<PlaylistItemDto> UpdatePlaylistItem([FromBody] PlaylistItemDto dto)
        {
            try
            {
                dto.CreatedBy = GetCurrentEmail();
                var result = await _playlistService.UpdatePlaylistItem(dto);
                return DtoHelper.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update playlist item", ex);
                throw;
            }
        }

        // Put updatePlaylistItem
        [HttpPut("removePlaylistItem")]
        public async Task<PlaylistDto> RemovePlaylistItem([FromBody] RemovePlaylistItemDto dto)
        {
            try
            {
                var result = await _playlistService.RemovePlaylistItem(dto);
                return DtoHelper.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to remove playlist item", ex);
                throw;
            }
        }

        // Post publish
        [HttpPost("publish/devices")]
        public async Task<bool> PublishPlaylist(PublishToDevicesDto publishDto)
        {
            try
            {
                Guid playlistId = publishDto.PlaylistId;
                string[] serialNumbers = publishDto.SerialNumbers;;
                var playlist = await _playlistService.Validate(playlistId);
                var dto = PopulatePlaylistDto(playlist);
                foreach (var serialNumber in serialNumbers)
                {
                    var device = await _deviceService.FindDeviceBySerialNumber(serialNumber);

                    var messageIdentity = await GetUserIdentity();
                    _messageClient.Publish(new Message(messageIdentity)
                        {
                            Topic = MessageTopics.PiPlaylistPublished,
                            Body = SerializeHelper.Stringify(dto)
                        }, MessageExchanges.DeviceExchange,
                        $"{messageIdentity.Organization}.{device.DeviceGroup.Name}.{serialNumber}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to publish playlist {playlistId}, {serialNumbers}", publishDto.PlaylistId, publishDto.SerialNumbers);
                throw;
            }
        }

        // Post publish
        [HttpPost("publish/group")]
        public async Task<bool> PublishPlaylist(PublishToGroupDto publishDto)
        {
            try
            {
                Guid playlistId = publishDto.PlaylistId;
                string groupName = publishDto.GroupName;
                var playlist = await _playlistService.Validate(playlistId);
                var dto = PopulatePlaylistDto(playlist);
                var messageIdentity = await GetUserIdentity();

                _messageClient.Publish(new Message(messageIdentity)
                {
                    Topic = MessageTopics.PiPlaylistPublished,
                    Body = SerializeHelper.Stringify(dto)
                }, MessageExchanges.DeviceExchange, $"{messageIdentity.Organization}.{groupName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to publish playlist {playlistId}, {groupName}", publishDto.PlaylistId, publishDto.GroupName);
                throw;
            }
        }

        // Post publish
        [HttpPost("publish/{id}")]
        public async Task<bool> PublishPlaylist(Guid id)
        {
            try
            {
                var playlist = await _playlistService.Validate(id);
                var dto = PopulatePlaylistDto(playlist);
                var messageIdentity = await GetUserIdentity();
                foreach (var deviceGroup in dto.DeviceGroups)
                {
                    _messageClient.Publish(new Message(messageIdentity)
                    {
                        Topic = MessageTopics.PiPlaylistPublished,
                        Body = SerializeHelper.Stringify(dto)
                    }, MessageExchanges.DeviceExchange, $"{messageIdentity.Organization}.{deviceGroup.Name}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to publish playlist {playlistId}", id);
                throw;
            }
        }

        private PlaylistDto PopulatePlaylistDto(Playlist model)
        {
            var dto = DtoHelper.Convert(model);
            dto.SubPlaylists = model.SubPlaylists.Select(pi => DtoHelper.Convert(pi)).ToList();
            dto.DeviceGroups = model.PlaylistGroups.Select(pg => DtoHelper.Convert(pg.DeviceGroup)).ToList();
            foreach (var subPlaylist in dto.SubPlaylists)
            {
                foreach (var itemDto in subPlaylist.PlaylistItems)
                {
                    var asset = _playlistService.FindAsset(itemDto.MediaAssetId, itemDto.AssetDiscriminator);
                    itemDto.Media = DtoHelper.Convert(asset);
                }
            }

            return dto;
        }

        // DELETE delete/id
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                bool result = _playlistService.DeletePlaylist(id);
                if (!result)
                {
                    return NotFound(null);
                }
                else
                {
                    return Ok(new DummyResult());
                }
            }
            catch (Exception e)
            {
                _logger.Error("Failed to delete playlist with id " + id, e);
                throw;
            }
        }
    }
}
