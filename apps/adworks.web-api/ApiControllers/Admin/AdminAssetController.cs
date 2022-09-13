using System;
using System.Linq;
using System.Threading.Tasks;
using adworks.data_services;
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
    /// <summary>
    /// TODO needs to have admin clams to access this controller
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/admin")]
    public class AdminAssetController : BaseApiController
    {
        private readonly ICommonDataService _commonDataService;
        private readonly IVideoService _videoService;
        private readonly IAudioService _audioService;
        private readonly IImageService _imageService;
        private readonly IFileService _fileService;
        private readonly IFtpService _ftpService;
        private readonly string _videoFolder;
        private readonly string _audioFolder;
        private readonly string _imageFolder;

        // Get the default form options so that we can use them to set the default limits for
        // request body data

        public AdminAssetController(IHubContext<EventHub> hubContext, ICommonDataService commonDataService,
            IVideoService videoService,  IAudioService audioService,  IImageService imageService, IUserService userService,
            IFileService fileService, IConfiguration configuration, IFtpService ftpService, ILogger logger) : base(configuration, userService, logger)
        {
            _videoFolder = this._configuration["Ftp:VideoFolder"];
            _audioFolder = this._configuration["Ftp:AudioFolder"];
            _imageFolder = this._configuration["Ftp:ImageFolder"];
            _commonDataService = commonDataService;
            _videoService = videoService;
            _audioService = audioService;
            _imageService = imageService;
            _fileService = fileService;
            _ftpService = ftpService;
        }

        [HttpGet("videos")]
        public async Task<IActionResult> GetVideos(string category = "",
            int pageIndex = 0,
            int pageSize = Int32.MaxValue,
            string orderBy = "CreatedOn",
            bool isDescending = true)
        {
            var videos = await _videoService.FindAll(category, pageIndex, pageSize, orderBy, isDescending);
            return Ok(videos.Select(DtoHelper.Convert));
        }

        [HttpGet("audios")]
        public async Task<IActionResult> GetAudios()
        {
            var audios = await _audioService.FindAll();
            return Ok(audios.Select(DtoHelper.Convert));
        }

        [HttpGet("images")]
        public async Task<IActionResult> GetImages()
        {
            var images = await _imageService.FindAll();
            return Ok(images.Select(img => DtoHelper.Convert(img)));
        }
    }
}
