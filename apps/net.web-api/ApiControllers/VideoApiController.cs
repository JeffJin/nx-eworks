using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
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
    [Route("api/videos")]
    public class VideoApiController : BaseApiController
    {
        private readonly IVideoService _videoService;
        private readonly IFileService _fileService;
        private readonly IMessageClient _messageClient;
        private readonly IFtpService _ftpService;
        private readonly string _videoFolder;
        private readonly string _ftpProcessorAddress;
        private readonly string _ftpStreamingAddress;

        // Get the default form options so that we can use them to set the default limits for
        // request body data

        public VideoApiController(IHubContext<EventHub> hubContext, IVideoService videoService, IFileService fileService,
            IConfiguration configuration, IUserService userService, IMessageClient messageClient,
            IFtpService ftpService, ILogger logger): base(configuration, userService, logger)
        {
            _videoFolder = _configuration["Ftp:VideoFolder"];
            _ftpProcessorAddress = _configuration["Ftp:ProcessorAddress"];
            _ftpStreamingAddress = _configuration["Ftp:StreamingAddress"];
            _videoService = videoService;
            _fileService = fileService;
            _messageClient = messageClient;
            _ftpService = ftpService;
        }

        // GET
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<VideoDto>> Get(string category = "",
            int pageIndex = 0,
            int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn",
            bool isDescending = true)
        {
            string email = GetCurrentEmail();
            var videos = await _videoService.FindVideos(email,
                category, pageIndex, pageSize, orderBy, isDescending);
            return videos.Select(DtoHelper.Convert);
        }

        [HttpGet("unprocessed")]
        public async Task<IEnumerable<MediaAssetDto>> GetUnprocessedVideos()
        {
            string email = GetCurrentEmail();
            var videos = await _videoService.FindUnprocessedVideos(email);
            return videos.Select(video => DtoHelper.Convert(video));
        }

        // GET api/video/details/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<VideoDto> GetVideoDetails(Guid id)
        {
            var video = await _videoService.FindVideo(id);

            return DtoHelper.Convert(video);
        }

        // GET api/video/5/thumbnails
        [HttpGet("thumbnails/{id}")]
        [AllowAnonymous]
        public async Task<IEnumerable<string>> GetThumbnails(Guid id)
        {
            var video = await _videoService.FindVideo(id);

            return _ftpService.ListThumbnails(video.CreatedBy.Email, id.ToString(), _ftpStreamingAddress);
        }

        // PUT api/video/update/5
        [HttpPut("{id}")]
        public async Task<VideoDto> Put([FromBody] VideoDto video)
        {
            try
            {
                video.UpdatedBy = GetCurrentEmail();

                var result = await _videoService.UpdateVideo(video);
                return DtoHelper.Convert(result);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update video", ex);
                throw;
            }

        }

        // POST api/video/addtext/5
        [HttpPut("addtext")]
        public async Task Post([FromBody] VideoAddText dto)
        {
            try
            {
                dto.CreatedBy = GetCurrentEmail();

                var messageIdentity = await GetUserIdentity();

                _messageClient.Publish(new Message(messageIdentity)
                {
                    Topic = MessageTopics.VideoAddText,
                    Body = SerializeHelper.Stringify(dto)
                }, MessageExchanges.ProcessorExchange, $"{messageIdentity.Organization}.{messageIdentity.Group}.*");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to add text to video", ex);
                throw;
            }

        }

        // DELETE api/video/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            string email = GetCurrentEmail();
            try
            {
                //TODO check if the user has access to the resource to be deleted.
                var result = _videoService.DeleteVideo(id);

                try
                {
                    //TODO check if the user has access to the resource to be deleted.
                    _fileService.RemoveVideo(id);

                    string path = _videoFolder + "/" + email + "/" + id;
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
                _logger.Error("Failed to delete video with id " + id, e);
                throw;
            }
        }
    }
}
