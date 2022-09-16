using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NReco.VideoInfo;

namespace adworks.media_common
{
    public interface IFFMpegService
    {
        Task<string> EncodeVideo(Guid videoId, string assetPath, MessageIdentity messageIdentity = null);
        Task<string> EncodeAudio(Guid auidoId, string assetPath, MessageIdentity messageIdentity = null);
        MediaInfo GetMediaInfo(string path);
        IList<string> CreateThumbnails(string fileUrl, int numberOfThumbnails, double duration);

        string AddTextToVideo(Guid videoId, string text, string filePath, string color="blue", int fontSize = 25);
        string AddAudioToImage(Guid imageId, string imagePath, string audioPath, int duration);
    }
}
;