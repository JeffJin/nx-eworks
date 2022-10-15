using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.media_common.Configuration;
using adworks.message_bus;
using adworks.message_common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
using ILogger = Serilog.ILogger;


namespace adworks.networking
{
    public class FtpService : IFtpService
    {
        private readonly IMessageClient _messageClient;
        private readonly ILogger _logger;
        private IConfiguration _configuration;
        private string _ftpStramingAddress;
        private string _ftpUserName;
        private string _ftpPassword;
        private string _videoFolder;
        private string _audioFolder;
        private string _imageFolder;
        private string _httpBaseUrl;
        private string _ftpAddress;
        private bool _ftpEnableSsl = false;

        public FtpService(ILogger logger,
            IMessageClient messageClient,
            IConfiguration configuration)
        {
            _messageClient = messageClient;
            _logger = logger;
            this._configuration = configuration;

            var ftpSettings = this._configuration.GetSection("Ftp").Get<FtpSettings>();
            _ftpUserName = ftpSettings.UserName;
            _ftpPassword = ftpSettings.Password;
            _videoFolder = ftpSettings.VideoFolder;
            _audioFolder = ftpSettings.AudioFolder;
            _imageFolder = ftpSettings.ImageFolder;
            _ftpEnableSsl = ftpSettings.Ssl ?? false;
            _ftpAddress = _configuration["Ftp:Address"];

            _httpBaseUrl = this._configuration["Video:HttpBaseUrl"];

        }

        public IEnumerable<string> ListThumbnails(string email, string videoId, string ftpAddress)
        {
            string ftpUrl = _videoFolder + "/" + email + "/" + videoId + "/thumbnails/";
            var list = ListFiles(ftpUrl, ftpAddress);
            return list.Select(l => _httpBaseUrl + email + "/" + videoId + "/thumbnails/" + l);
        }

        public void DeleteDirectoryAndFiles(string dirName, string ftpAddress)
        {
            var files = ListFiles(dirName, ftpAddress);
            foreach (var file in files)
            {
                DeleteFile(dirName + "/" + file, ftpAddress);
            }

            var folders = ListDirectories(dirName, ftpAddress);
            foreach (var folder in folders)
            {
                DeleteDirectoryAndFiles(dirName + "/" + folder, ftpAddress);
            }

            DeleteDirectory(dirName, ftpAddress);
        }

        public IList<string> ListFiles(string directory, string ftpAddress)
        {
            var list = ListDirectory(directory, ftpAddress);
            return list.Where(item => item.StartsWith("-r")).Select(t =>
            {
                var temp = t.Split(' ');
                return StringHelper.RemoveInvalidPathChars(temp[temp.Length - 1]);
            }).ToList();
        }

        public IList<string> ListDirectories(string directory, string ftpAddress)
        {
            var list = ListDirectory(directory, ftpAddress);
            return list.Where(item => item.StartsWith("dr")).Select(t =>
            {
                var temp = t.Split(' ');
                return StringHelper.RemoveInvalidPathChars(temp[temp.Length - 1]);
            }).ToList();
        }

        public void DeleteDirectory(string dirName, string ftpAddress)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest) FtpWebRequest.Create(ftpAddress + dirName);

                request.EnableSsl = _ftpEnableSsl;
                request.UsePassive = true;
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);

                using (var resp = (FtpWebResponse) request.GetResponse())
                {
                    _logger.Information(resp.StatusDescription, resp.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Remove directory failed", dirName);
                throw;
            }
        }

        public void DeleteFile(string ftpFile, string ftpAddress)
        {
            try
            {
                FtpWebRequest request = (System.Net.FtpWebRequest) FtpWebRequest.Create(ftpAddress + ftpFile);
                request.EnableSsl = _ftpEnableSsl;
                request.UsePassive = true;
                request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                using (var resp = (FtpWebResponse) request.GetResponse())
                {
                    _logger.Information(resp.StatusDescription, resp.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Remove file failed", ftpFile);
                throw;
            }
        }

        /// <summary>
        /// Upload files and sub-folders under the specified folder, excluding the folderPath
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="destFolder"></param>
        /// <param name="ftpAddress"></param>
        /// <param name="messageIdentity"></param>
        public void UploadFolder(string folderPath, string destFolder, string ftpAddress, MessageIdentity messageIdentity)
        {
            var files = Directory.GetFiles(folderPath);
            UploadFiles(files, destFolder, ftpAddress, messageIdentity);

            var folders = Directory.GetDirectories(folderPath);
            foreach (var folder in folders)
            {
                var temp = folder.Split('/');
                string currentFolderName = temp[temp.Length - 1];
                UploadFolder(folder, Path.Combine(destFolder, currentFolderName), ftpAddress, messageIdentity);
            }
        }

        private void UploadFiles(string[] files, string dest, string ftpAddress, MessageIdentity messageIdentity)
        {
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);

                if (fileName.StartsWith(".") || !FileTypeHelper.IsMediaFile(fileName))
                {
                    continue;
                }

                Upload(file, dest, ftpAddress, messageIdentity);
            }
        }

        /// <summary>
        /// create single directory
        /// </summary>
        /// <param name="folderName"></param>
        private bool MakeDirectory(string folderName, string ftpAddress)
        {
            if (!folderName.StartsWith("/"))
            {
                folderName = "/" + folderName;
            }

            try
            {
                FtpWebRequest request = (FtpWebRequest) FtpWebRequest.Create(ftpAddress + folderName);

                request.EnableSsl = _ftpEnableSsl;
                request.UsePassive = true;
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);

                using (var resp = (FtpWebResponse) request.GetResponse())
                {
                    _logger.Information(resp.StatusDescription, resp.StatusCode.ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Create directory failed {folderName}");
                return false;
            }
        }

        /// <summary>
        /// create directory recursivelly
        /// </summary>
        /// <param name="dirName"></param>
        public bool CreateDirectory(string dirName, string ftpAddress)
        {
            var paths = dirName.Split('/');
            string currentPath = "";
            for (var i = 0; i < paths.Length; i++)
            {
                if (!string.IsNullOrEmpty(paths[i]))
                {
                    currentPath = Path.Combine(currentPath, paths[i]);
                    if (DirectoryExists(currentPath, ftpAddress))
                    {
                        continue;
                    }

                    if (!MakeDirectory(currentPath, ftpAddress))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public IList<string> ListDirectory(string folder, string ftpAddress)
        {
            string folderAddress = ftpAddress + folder;
            var list = new List<string>();
            try
            {
                FtpWebRequest request = (FtpWebRequest) FtpWebRequest.Create(folderAddress);

                request.EnableSsl = _ftpEnableSsl;
                request.UsePassive = true;
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);
                using (var resp = (FtpWebResponse) request.GetResponse())
                {
                    Stream responseStream = resp.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);
                    string result = reader.ReadToEnd();
                    var temp = result.Split(new string[] {Environment.NewLine}, StringSplitOptions.None);
                    foreach (var path in temp)
                    {
                        list.Add(path);
                    }

                    _logger.Information("Directory List Complete, status {0}", resp.StatusDescription);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to list directory {folder}");
                throw;
            }

            return list;
        }

        public void Upload(string file, string destFolder, string ftpAddress, MessageIdentity messageIdentity)
        {
            string mediaType = FileTypeHelper.GetMediaType(file);
            string fileName = Path.GetFileName(file);
            fileName = StringHelper.RemoveInvalidPathChars(fileName);
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException(file + " is not valid file path");
            }

            _logger.Information($"Uploading file {fileName} to destination {destFolder}");

            if (_messageClient != null && messageIdentity != null)
            {
                _messageClient.Publish(new Message(messageIdentity)
                {
                    Topic = string.Format(MessageTopics.TemplateTransferToFtpSite, mediaType),
                    Body = SerializeHelper.Stringify(new FtpEvent()
                    {
                        SourceFile = fileName,
                        Destination = destFolder
                    })
                }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}");
            }

            var folderCreated = CreateDirectory(destFolder, ftpAddress);
            if (!folderCreated)
            {
                throw new InvalidOperationException("unable to create folder " + destFolder);
            }

            if (!destFolder.EndsWith("/"))
            {
                destFolder = destFolder + "/";
            }

            if (!destFolder.StartsWith("/"))
            {
                destFolder = "/" + destFolder;
            }

            try
            {
                FtpWebRequest ftp = (FtpWebRequest) WebRequest.Create(ftpAddress + destFolder + fileName);

                ftp.EnableSsl = _ftpEnableSsl;
                ftp.UsePassive = true;
                ftp.Method = WebRequestMethods.Ftp.UploadFile;
                ftp.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);
                ftp.KeepAlive = true;
                ftp.UseBinary = true;

                using (FileStream inputStream = File.OpenRead(file))
                using (Stream ftpstream = ftp.GetRequestStream())
                {
                    var buffer = new byte[512 * 512];
                    int totalReadBytesCount = 0;
                    int readBytesCount;
                    while ((readBytesCount = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        int progress = Convert.ToInt16(totalReadBytesCount * 100.0 / inputStream.Length);
                        ftpstream.Write(buffer, 0, readBytesCount);
                        totalReadBytesCount += readBytesCount;
                        var evt = new ProgressEvent
                        {
                            JobName = fileName,
                            Progress = progress
                        };
                        if (_messageClient != null && messageIdentity != null)
                        {
                            _messageClient.Publish(new Message(messageIdentity)
                            {
                                Topic = string.Format(MessageTopics.TemplateTransferToFtpSiteProgress, mediaType),
                                Body = SerializeHelper.Stringify(evt)
                            }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}");
                        }

                    }

                    if (_messageClient != null && messageIdentity != null)
                    {
                        _messageClient.Publish(new Message(messageIdentity)
                        {
                            Topic = string.Format(MessageTopics.TemplateTransferToFtpSiteProgress, mediaType),
                            Body = SerializeHelper.Stringify(new ProgressEvent
                            {
                                JobName = fileName,
                                Progress = 100
                            })
                        }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Upload file failed", file);
                throw;
            }
        }

        public async Task<string> UploadAsync(string file, string destFolder, string ftpAddress, MessageIdentity messageIdentity)
        {
            string fileName = Path.GetFileName(file);
            fileName = StringHelper.RemoveInvalidPathChars(fileName);
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException(file + " is not valid file path");
            }

            _logger.Information($"Uploading file {fileName} to destination {destFolder}");

            var folderCreated = CreateDirectory(destFolder, ftpAddress);
            if (!folderCreated)
            {
                throw new InvalidOperationException("unable to create folder " + destFolder);
            }

            if (!destFolder.EndsWith("/"))
            {
                destFolder += "/";
            }

            if (!destFolder.StartsWith("/"))
            {
                destFolder = "/" + destFolder;
            }

            var finalPath = destFolder + fileName;
            try
            {
                FtpWebRequest ftpWebRequest = (FtpWebRequest) WebRequest.Create(ftpAddress + finalPath);

                ftpWebRequest.EnableSsl = _ftpEnableSsl;
                ftpWebRequest.UsePassive = true;
                ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpWebRequest.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);
                ftpWebRequest.KeepAlive = true;
                ftpWebRequest.UseBinary = true;

                using (FileStream inputStream = File.OpenRead(file))
                using (Stream ftpstream = ftpWebRequest.GetRequestStream())
                {
                    var buffer = new byte[512 * 512];
                    int totalReadBytesCount = 0;
                    int readBytesCount;

                    while ((readBytesCount = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        await ftpstream.WriteAsync(buffer, 0, readBytesCount);
                        totalReadBytesCount += readBytesCount;
                        var progress = totalReadBytesCount * 100.0 / inputStream.Length;
                        //report progress
                        this._logger.Debug($"ftp upload progress for total {inputStream.Length}, current progress {progress}");
                        NotifyClients(progress, messageIdentity);
                    }

                }

                return ftpWebRequest.RequestUri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Upload file failed", file);
                throw;
            }
        }

        private void NotifyClients(double progress, MessageIdentity messageIdentity)
        {
            if (_messageClient != null && messageIdentity != null)
            {
                _messageClient.Publish(new Message(messageIdentity)
                {
                    Topic = "FTPEvent",
                    Body = SerializeHelper.Stringify(new ProgressEvent
                    {
                        JobName = "UploadingToFTP",
                        Progress = progress
                    })
                }, MessageExchanges.UserExchange, $"{messageIdentity.ToString()}");
            }
        }

        public bool DirectoryExists(string directoryPath, string ftpAddress)
        {
            if (!directoryPath.EndsWith("/"))
            {
                directoryPath = directoryPath + "/";
            }

            if (!directoryPath.StartsWith("/"))
            {
                directoryPath = "/" + directoryPath;
            }

            bool isExists = true;
            try
            {
                var request = (FtpWebRequest) WebRequest.Create(ftpAddress + directoryPath);

                request.EnableSsl = _ftpEnableSsl;
                request.UsePassive = true;
                request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                FtpWebResponse response = (FtpWebResponse) request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string result = reader.ReadToEnd();
            }
            catch (Exception)
            {
                isExists = false;
            }

            return isExists;
        }

        public string Download(string cloudUrl)
        {
            var baseAssetFolder = this._configuration["BaseAssetFolder"];
            string tempFileName = Guid.NewGuid().ToString();
            string ext = Path.GetExtension(cloudUrl);
            ext = string.IsNullOrEmpty(ext) ? ".mp4" : ext;
            string tempFolder = Path.Combine(baseAssetFolder, "temp");
            string tempPath = Path.Combine(tempFolder, $"{tempFileName}{ext}");
            var request = (FtpWebRequest)WebRequest.Create(cloudUrl);
            request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.UsePassive = true;
            _logger.Information($"start downloading file from {cloudUrl} to {tempPath} with credentials {_ftpUserName} {_ftpPassword}");
            try
            {
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                using (Stream fileStream = File.Create(tempPath))
                {
                    ftpStream.CopyTo(fileStream);
                }

                return tempPath;
            }
            catch (Exception e)
            {
                _logger.Error($"Download file {cloudUrl} failed", e);
                throw;
            }

        }

        public bool FileExists(string filePath, string ftpAddress)
        {
            bool isExists = true;
            try
            {
                var request = (FtpWebRequest) WebRequest.Create(ftpAddress + filePath);

                request.EnableSsl = _ftpEnableSsl;
                request.UsePassive = true;
                request.Credentials = new NetworkCredential(_ftpUserName, _ftpPassword);
                request.Method = WebRequestMethods.Ftp.GetFileSize;

                FtpWebResponse response = (FtpWebResponse) request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string result = reader.ReadToEnd();
            }
            catch (Exception)
            {
                isExists = false;
            }

            return isExists;
        }
    }
}
