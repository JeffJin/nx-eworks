using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;
using adworks.message_bus;
using adworks.networking;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/audios")]
    public class AudioApiController : BaseApiController
    {
        private readonly IAudioService _audioService;
        private readonly IFileService _fileService;
        private readonly IFtpService _ftpService;
        private readonly string _audioFolder;
        private string _ftpStreamingAddress;

        // Get the default form options so that we can use them to set the default limits for
        // request body data

        public AudioApiController(IHubContext<EventHub> hubContext, IAudioService audioService,
            IFileService fileService, IConfiguration configuration, IUserService userService,
            IFtpService ftpService, ILogger logger): base(configuration, userService, logger)
        {
            _audioFolder = this._configuration["Ftp:AudioFolder"];
            _audioService = audioService;
            _fileService = fileService;
            _ftpService = ftpService;
            _ftpStreamingAddress = _configuration["Ftp:StreamingAddress"];
        }

        // GET api/audio/list
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<AudioDto>> Get()
        {
            var claim= TryGetClaim(HttpContext.User , ClaimTypes.Email);
            string email = claim.Value;

            IEnumerable<Audio> audios = await _audioService.FindAudios(email);
            return audios.Select(m => DtoHelper.Convert(m));
        }

        // GET api/audio/metadata/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<AudioDto> GetDetails(Guid id)
        {
            Audio audio = await _audioService.FindAudio(id);

            return DtoHelper.Convert(audio);
        }

        // PUT api/audio/update/5
        [HttpPut("{id}")]
        public async Task<AudioDto> Put([FromBody] AudioDto dto)
        {
            try
            {
                dto.UpdatedBy = GetCurrentEmail();

                var result = await _audioService.UpdateAudio(dto);
                return DtoHelper.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update audio", ex);
                throw;
            }

        }

        // DELETE api/audio/delete/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var claim = TryGetClaim(HttpContext.User, ClaimTypes.Email);
            string email = claim.Value;
            try
            {
                var result = _audioService.DeleteAudio(id);

                try
                {
                    //TODO check if the user has access to the resource to be deleted.
                    _fileService.RemoveAudio(id);
                    string path = _audioFolder + "/" + email + "/" + id;
                    //WARNING if ftp deletion fails for some reason, they may become dangling records
                    _ftpService.DeleteDirectoryAndFiles(path, _ftpStreamingAddress);
                }
                catch (Exception)
                {
                    //swallow file deletion error if any and just log error into the file system
                    //as long as database record is deleted, the entity is no longer available
                }
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
                _logger.Error("Failed to delete audio with id " + id, e);
                return BadRequest();
            }
        }
    }
}
