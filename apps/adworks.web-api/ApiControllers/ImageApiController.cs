using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;
using adworks.message_bus;
using adworks.message_common;
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
    [Route("api/images")]
    public class ImageApiController : BaseApiController
    {
        private readonly IImageService _imageService;
        private readonly IFileService _fileService;
        private readonly IFtpService _ftpService;
        private readonly IMessageClient _messageClient;
        private readonly string _imageFolder;
        private string _ftpProcessorAddress;
        private string _ftpStreamingAddress;

        // Get the default form options so that we can use them to set the default limits for
        // request body data

        public ImageApiController(IHubContext<EventHub> hubContext, ICommonDataService commonDataService,
            IImageService imageService, IFileService fileService, IConfiguration configuration, IUserService userService,
            IFtpService ftpService, IMessageClient messageClient, ILogger logger): base(configuration, userService, logger)
        {
            _imageFolder = this._configuration["Ftp:ImageFolder"];
            _imageService = imageService;
            _fileService = fileService;
            _ftpService = ftpService;
            _messageClient = messageClient;
            _ftpProcessorAddress = _configuration["Ftp:ProcessorAddress"];
            _ftpStreamingAddress = _configuration["Ftp:StreamingAddress"];
        }

        // GET api/images
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<ImageDto>> Get(string category = "",
            int pageIndex = 0,
            int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn",
            bool isDescending = true)
        {
            string email = GetCurrentEmail();

            IEnumerable<Image> images = await _imageService.FindImages(email,
                category, pageIndex, pageSize, orderBy, isDescending);
            return images.Select(m => DtoHelper.Convert(m));
        }

        // GET api/images/2505d672-2a15-4e26-b1de-30d29bfab9de
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ImageDto> GetDetails(Guid id)
        {
            Image image = await _imageService.FindImage(id);

            return DtoHelper.Convert(image);
        }

        // PUT api/image/merge/5
        [HttpPost("merge/{imageId}")]
        public async Task MergeAudio(Guid imageId, Guid audioId, int duration)
        {
            try
            {
                var userDto = GetCurrentUser();
                var messageIdentity = await GetUserIdentity();

                _messageClient.Publish(new Message(messageIdentity)
                {
                    Topic = MessageTopics.ImageMergeAudio,
                    Body = SerializeHelper.Stringify(new MergeAudioDto
                    {
                        ImageId = imageId,
                        AudioId = audioId,
                        Duration = duration,
                        User = userDto
                    })
                }, MessageExchanges.ProcessorExchange, $"{messageIdentity.ToString()}");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update image", ex);
                throw;
            }
        }

        // PUT api/image/update/5
        [HttpPut("{id}")]
        public async Task<ImageDto> Put([FromBody] ImageDto dto)
        {
            try
            {
                dto.UpdatedBy = GetCurrentEmail();

                var result = await _imageService.UpdateImage(dto);
                return DtoHelper.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update image", ex);
                throw;
            }
        }

        // DELETE api/image/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            string email = GetCurrentEmail();
            try
            {
                //TODO check if the user has access to the resource to be deleted.
                var result = _imageService.DeleteImage(id);

                try
                {
                    //TODO check if the user has access to the resource to be deleted.
                    _fileService.RemoveImage(id);

                    string path = _imageFolder + "/" + email + "/" + id;
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
                _logger.Error("Failed to delete image with id " + id, e);
                throw;
            }
        }
    }
}
