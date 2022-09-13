using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using adworks.media_common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.networking
{
    public class FileService : IFileService
    {
        private readonly ILogger _logger;
        private IConfiguration _configuration;
        private readonly IFtpService _ftpService;
        private string _ftpHomeFolder;
        private string _convertedVideoFolder;
        private string _convertedAudioFolder;
        private string _convertedImageFolder;
        private string _uploadedVideoFolder;
        private string _uploadedAudioFolder;
        private string _uploadedImageFolder;

        public FileService(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var baseAssetFolder = this._configuration["BaseAssetFolder"];

            if (!string.IsNullOrWhiteSpace(_ftpHomeFolder) && !_ftpHomeFolder.StartsWith("/"))
            {
                var workingDirectory  = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                _ftpHomeFolder = Path.Combine(workingDirectory, _ftpHomeFolder);
            }

            this._convertedVideoFolder = Path.Combine(baseAssetFolder, this._configuration["Video:ConvertedFolder"]);
            this._convertedAudioFolder = Path.Combine(baseAssetFolder, this._configuration["Audio:ConvertedFolder"]);
            this._convertedImageFolder = Path.Combine(baseAssetFolder, this._configuration["Image:ConvertedFolder"]);
            this._uploadedVideoFolder = Path.Combine(baseAssetFolder, this._configuration["Video:UploadFolder"]);
            this._uploadedAudioFolder = Path.Combine(baseAssetFolder, this._configuration["Audio:UploadFolder"]);
            this._uploadedImageFolder = Path.Combine(baseAssetFolder, this._configuration["Image:UploadFolder"]);
        }

        public IEnumerable<string> FindThumbnails(string videoId)
        {
            var thumbnailFolderName = this._configuration["Video:ThumbnailFolderName"];
            var files = Directory.GetFiles(Path.Combine(this._convertedVideoFolder, videoId, thumbnailFolderName));
            return files.Where(f => f.Contains(videoId) && f.EndsWith("jpg"));
        }


        public async Task<IList<string>> UploadFormFiles(List<IFormFile> files)
        {
            if (string.IsNullOrEmpty(this._uploadedVideoFolder))
            {
                throw new InvalidDataException("Output folder is not specified");
            }

            var list = new List<string>();

            for (int i = 0; i < files.Count; i++)
            {
                var formFile = files[i];
                if (formFile.Length > 0)
                {
                    var path = Path.Combine(this._uploadedVideoFolder, formFile.FileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        //TODO get file upload progress
                        await formFile.CopyToAsync(stream);
                    }

                    list.Add(path);
                }
            }
            return list;
        }

        public async Task<FileDto> UploadFile(Stream sourceStream, string inputFileName, string uploadFolder)
        {
            string fileName = StringHelper.CleanupFileName(inputFileName);

            var fileData = new FileDto()
            {
                Id = Guid.NewGuid()
            };

            var targetFolder = Path.Combine(uploadFolder, fileData.Id.ToString());
            try
            {
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }
                var targetFilePath = Path.Combine(targetFolder, fileName);

                await using var targetStream = File.Create(targetFilePath);
                await sourceStream.CopyToAsync(targetStream);
                fileData.RawFilePath = targetFilePath;
                fileData.FileSize = new FileInfo(targetFilePath).Length;
                _logger.Information($"Copied the uploaded file '{targetFilePath}'");
            }
            catch (Exception e)
            {
                _logger.Error(e, "unable to upload file", inputFileName);
                throw;
            }

            return fileData;
        }

        private static Encoding GetEncoding(string contentType)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(contentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }

        public void FillVideoData(StringSegment contentName, string contentType, Stream sourceStream,
            int fieldLimit, ref VideoDto videoData, ref IDictionary<string, string> formAccumulator)
        {
            // Do not limit the key name length here because the
            // multipart headers length limit is already in effect.
            var key = HeaderUtilities.RemoveQuotes(contentName);
            var encoding = GetEncoding(contentType);
            using (var streamReader = new StreamReader(
                sourceStream,
                encoding,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true))
            {
                // The value length limit is enforced by MultipartBodyLengthLimit
                var value = streamReader.ReadToEnd();
                if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                {
                    value = String.Empty;
                }
                formAccumulator.Add(key.Value, value);

                if (formAccumulator.Count > fieldLimit)
                {
                    throw new InvalidDataException(
                        $"Form key count limit {fieldLimit} exceeded.");
                }

                if (key.Value == "title")
                {
                    videoData.Title = value;
                }
                if (key.Value == "description")
                {
                    videoData.Description = value;
                }
                if (key.Value == "category")
                {
                    videoData.Category = value;
                }
                if (key.Value == "tags")
                {
                    videoData.Tags = value;
                }
            }
        }

        public void RemoveVideo(Guid videoId)
        {
            try
            {
                string uploadedFolder =
                    Path.Combine(this._uploadedVideoFolder, videoId.ToString());
                string convertedFolder =
                    Path.Combine(this._convertedVideoFolder, videoId.ToString());
                if (Directory.Exists(uploadedFolder))
                {
                    Directory.Delete(uploadedFolder, true);
                }
                if (Directory.Exists(convertedFolder))
                {
                    Directory.Delete(convertedFolder, true);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "failed to remove video file and folder", videoId);
                throw;
            }

        }
        public void RemoveImage(Guid imageId)
        {
            try
            {
                string uploadedFolder =
                    Path.Combine(this._uploadedImageFolder, imageId.ToString());
                if (Directory.Exists(uploadedFolder))
                {
                    Directory.Delete(uploadedFolder, true);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "failed to remove image file and folder", imageId);
                throw;
            }

        }
        public void RemoveAudio(Guid audioId)
        {
            try
            {
                string uploadedFolder =
                    Path.Combine(this._uploadedAudioFolder, audioId.ToString());
                string convertedFolder =
                    Path.Combine(this._convertedAudioFolder, audioId.ToString());
                if (Directory.Exists(uploadedFolder))
                {
                    Directory.Delete(uploadedFolder, true);
                }
                if (Directory.Exists(convertedFolder))
                {
                    Directory.Delete(convertedFolder, true);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "failed to remove audio file and folder", audioId);
                throw;
            }

        }

        public void DeleteLocalFile(string localPath)
        {
            try
            {
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"failed to remove local file {localPath}");
                throw;
            }
        }

        public void DeleteLocalFolder(string localPathFolder)
        {
            try
            {
                if (Directory.Exists(localPathFolder))
                {
                    Directory.Delete(localPathFolder, true);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"failed to remove local folder {localPathFolder}");
                throw;
            }
        }

        /// <summary>
        /// upload file to local drive
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destFolder">destination folder is base folder/user email/videoId</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Upload(string file, string destFolder)
        {
            throw new NotImplementedException();
        }
    }
}
