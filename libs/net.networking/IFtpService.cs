using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using adworks.media_common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace adworks.networking
{
    public interface IFtpService
    {
        /// <summary>
        /// upload file to destination folder, remote ftp site with firing progress through message queue
        /// </summary>
        /// <param name="file">local file full path</param>
        /// <param name="destFolder">destination folder is base folder/user email/videoId</param>
        /// <param name="ftpAddress"></param>
        /// <param name="messageIdentity">message identity consists of destination type, organization, group and endpoint key </param>
        void Upload(string file, string destFolder, string ftpAddress, MessageIdentity messageIdentity);

        /// <summary>
        /// upload file to destination folder, remote ftp site
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destFolder"></param>
        /// <param name="ftpAddress"></param>
        /// <param name="messageIdentity"></param>
        Task<string>  UploadAsync(string file, string destFolder, string ftpAddress, MessageIdentity messageIdentity);
        
        /// <summary>
        /// delete directory and its containing contents
        /// </summary>
        /// <param name="dirName"></param>
        void DeleteDirectoryAndFiles(string dirName, string ftpAddress);

        /// <summary>
        /// upload folder and its contents(1 level down only)
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="destFolder"></param>
        /// <param name="ftpAddress"></param>
        /// <param name="messageIdentity">message identity consists of destination type, organization, group and endpoint key </param>
        void UploadFolder(string folderPath, string destFolder, string ftpAddress, MessageIdentity messageIdentity);

        /// <summary>
        /// get thumbnail links for a video
        /// </summary>
        /// <param name="email"></param>
        /// <param name="videoId"></param>
        /// <returns></returns>
        IEnumerable<string> ListThumbnails(string email, string videoId, string ftpAddress);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool FileExists(string filePath, string ftpAddress);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        bool DirectoryExists(string directoryPath, string ftpAddress);

        /// <summary>
        /// download to local path
        /// </summary>
        /// <param name="cloudUrl"></param>
        /// <returns></returns>
        string Download(string cloudUrl);
    }
}