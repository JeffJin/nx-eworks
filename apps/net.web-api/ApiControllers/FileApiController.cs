using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.media_common;
using adworks.media_web_api.Attributes;
using adworks.media_web_api.Helpers;
using adworks.message_bus;
using adworks.message_common;
using adworks.networking;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;
using Microsoft.Net.Http.Headers;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/files")]
    public class FileApiController : BaseApiController
    {
        private readonly string DefaultCategory = "Unprocessed";

        private readonly IHubContext<EventHub> _hubContext;
        private readonly IFileService _fileService;
        private readonly IMessageClient _messageClient;

        private readonly IVideoService _videoService;
        private readonly IImageService _imageService;
        private readonly IAudioService _audioService;
        private readonly IFtpService _ftpService;

        // Get the default form options so that we can use them to set the default limits for
        // request body data
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        private readonly string _uploadVideoFolder;
        private readonly string _uploadAudioFolder;
        private readonly string _uploadImageFolder;
        private readonly string _ftpHomeFolder;
        private string _ftpStreamingAddress;

        public FileApiController(IHubContext<EventHub> hubContext, IFileService fileService,
            IMessageClient messageClient, IVideoService videoService, IImageService imageService,
            IAudioService audioService, IUserService userService, IFtpService ftpService,
            IConfiguration configuration, ILogger logger): base(configuration, userService, logger)
        {
            this._uploadVideoFolder = Path.Combine(this._configuration["BaseAssetFolder"], this._configuration["Video:UploadFolder"]);
            this._uploadAudioFolder = Path.Combine(this._configuration["BaseAssetFolder"], this._configuration["Audio:UploadFolder"]);
            this._uploadImageFolder = Path.Combine(this._configuration["BaseAssetFolder"], this._configuration["Image:UploadFolder"]);
            this._ftpHomeFolder = this._configuration["Ftp:HomeFolder"];
            _ftpStreamingAddress = _configuration["Ftp:StreamingAddress"];

            _hubContext = hubContext;
            _fileService = fileService;
            _messageClient = messageClient;
            _videoService = videoService;
            _imageService = imageService;
            _audioService = audioService;
            _ftpService = ftpService;
        }

        // POST api/files/upload
        [HttpPost("upload")]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(20_000_000_000)]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            var dtoList = new List<MediaAssetDto>();
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader && MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    var dto = await UploadMediaAsset(contentDisposition, section.Body);
                    dtoList.Add(dto);
                }
                section = await reader.ReadNextSectionAsync();
            }

            return Ok(dtoList);
        }

        private async Task<MediaAssetDto> UploadMediaAsset(ContentDispositionHeaderValue contentDisposition, Stream stream)
        {
            var dto = new MediaAssetDto()
            {
                CreatedBy = GetCurrentEmail(),
                UpdatedBy = GetCurrentEmail()
            };
            string fileType = string.Empty;
            MediaAssetDto result = null;
            var messageIdentity = await GetUserIdentity();

            if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
            {
                string fileName = contentDisposition.FileName.Value;
                var folder = string.Empty;
                if (FileTypeHelper.IsVideo(fileName))
                {
                    folder = _uploadVideoFolder;
                    fileType = AssetTypes.Video;
                }
                else if (FileTypeHelper.IsAudio(fileName))
                {
                    folder = _uploadAudioFolder;
                    fileType = AssetTypes.Audio;
                }
                else if (FileTypeHelper.IsImage(fileName))
                {
                    folder = _uploadImageFolder;
                    fileType = AssetTypes.Image;
                }
                else {
                    return null;
                }
                var fileData = await _fileService.UploadFile(stream, fileName, folder);
                try
                {
                    //upload to FTP server on processor server
                    var destFolder = Path.Combine(_ftpHomeFolder, "uploaded", fileData.Id.ToString());

                    var resultPath = await _ftpService.UploadAsync(fileData.RawFilePath, destFolder, _ftpStreamingAddress, messageIdentity);
                    dto.Id = fileData.Id;
                    dto.Title = StringHelper.GetTitleFromFileName(fileName);
                    dto.CloudUrl = resultPath;
                    dto.RawFilePath = fileData.RawFilePath;
                    dto.Category = DefaultCategory;
                    dto.FileSize = fileData.FileSize;
                    dto.FileType = fileType;

                    _logger.Information($"Persists MediaAsset '{SerializeHelper.Stringify(dto)}', '{fileType}'");
                    return await PersistMediaAsset(dto, fileType);
                }
                catch (Exception ex)
                {
                    //TODO notify client side and clean up files and remove database records
                    return null;
                }
            }
            return null;
        }

        [HttpDelete("cleanup")]
        public IActionResult CleanupFile(MediaAssetDto dto)
        {
            try
            {
                if (!_ftpService.FileExists(dto.CloudUrl, _ftpStreamingAddress))
                {
                    //TODO mark this item as to be reviewed
                    return NotFound(null);
                }

                //if FTP upload is successful, remove local file
                string folderToDelete = Path.GetDirectoryName(dto.RawFilePath);
                Directory.Delete(folderToDelete, true);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to clean up the local item, {url} {filePath}",
                    dto.CloudUrl, dto.RawFilePath);
                throw;
            }

            return Ok(new DummyResult());
        }

        /// <summary>
        /// save the item into database before processing
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="assetType"></param>
        /// <returns></returns>
        private async Task<MediaAssetDto> PersistMediaAsset(MediaAssetDto dto, string assetType)
        {
            if (assetType == AssetTypes.Video)
            {
                var item =  await _videoService.AddVideo(new VideoDto(dto));
                dto.Id = item.Id;
            }
            else if (assetType == AssetTypes.Audio)
            {
                var item = await _audioService.AddAudio(new AudioDto(dto));
                dto.Id = item.Id;
            }
            else if (assetType == AssetTypes.Image)
            {
                var item = await _imageService.AddImage(new ImageDto(dto));
                dto.Id = item.Id;
            }

            return dto;
        }

        //notify rabbit mq server to start process media files
        [HttpPost("process")]
        public async Task<IActionResult> ProcessAssets([FromBody] MediaAssetDto[] dtoList)
        {
            var messages = new List<Message>();
            foreach (var dto in dtoList)
            {
                if (dto == null)
                {
                    continue;
                }
                var message = await ProcessAsset(dto);
                if (message != null)
                {
                    messages.Add(message);
                }
            }

            return Ok(messages);
        }

        private async Task<Message> ProcessAsset(MediaAssetDto dto)
        {
            string topic = string.Empty;
            string fileType = string.Empty;
            //verify if the asset is already persisted in database before processing
            if (FileTypeHelper.IsVideo(dto.CloudUrl))
            {
                var entity = await _videoService.FindVideo(dto.Id);
                topic = entity == null ? string.Empty : MessageTopics.VideoUploaded;
                fileType = "Video";
            }
            else if (FileTypeHelper.IsAudio(dto.CloudUrl))
            {
                var entity = await _audioService.FindAudio(dto.Id);
                topic = entity == null ? string.Empty : MessageTopics.AudioUploaded;
                fileType = "Audio";
            }
            else if (FileTypeHelper.IsImage(dto.CloudUrl))
            {
                var entity = await _imageService.FindImage(dto.Id);
                topic = entity == null ? string.Empty : MessageTopics.ImageUploaded;
                fileType = "Image";
            }
            else
            {
                topic = string.Empty;
                fileType = "Unknown";
            }

            if (string.IsNullOrEmpty(topic))
            {
                return null;
            }
            var messageIdentity = await GetUserIdentity();
            var message = new Message(messageIdentity)
            {
                Topic = topic,
                Body = SerializeHelper.Stringify(dto)
            };
            _messageClient.Publish(message, MessageExchanges.ProcessorExchange, $"{fileType}.{messageIdentity}");

            return message;
        }
    }
}
