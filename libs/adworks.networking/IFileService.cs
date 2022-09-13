using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using adworks.media_common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace adworks.networking
{
    public interface IFileService
    {
        IEnumerable<string> FindThumbnails(string videoId);
        Task<FileDto> UploadFile(Stream sourceStream, string inputFileName, string uploadFolder);
        Task<IList<string>> UploadFormFiles(List<IFormFile> files);
        void FillVideoData(StringSegment contentName, string contentType, Stream sourceStream,
            int fieldLimit, ref VideoDto videoData, ref IDictionary<string, string> formAccumulator);

        void RemoveVideo(Guid videoId);
        void RemoveImage(Guid imageId);
        void RemoveAudio(Guid audioId);
        void DeleteLocalFile(string localPath);
        public void DeleteLocalFolder(string localPathFolder);
    }
}