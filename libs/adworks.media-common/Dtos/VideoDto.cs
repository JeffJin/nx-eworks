using System;
using System.Collections.Generic;

namespace adworks.media_common
{
    public class VideoDto: MediaAssetDto
    {
        public VideoDto()
        {
            Thumbnails = new List<string>();
        }
        
        public VideoDto(MediaAssetDto assetDto)
        {
            RawFilePath = assetDto.RawFilePath;
            FileSize = assetDto.FileSize;
            Category = assetDto.Category;
            Title = assetDto.Title;
            Description = assetDto.Description;
            Tags = assetDto.Tags;
            CloudUrl = assetDto.CloudUrl ?? assetDto.RawFilePath; //temporary url
            MainThumbnail = assetDto.Id.ToString(); //temporary url
            Id = assetDto.Id;
            UpdatedOn = assetDto.UpdatedOn;
            UpdatedBy = assetDto.UpdatedBy;
            CreatedBy = assetDto.CreatedBy;
        }
        
        public string EncodedFilePath { get; set; }
        public string ProgressiveUrl { get; set; }
        public string HlsUrl { get; set; }
        public double Duration { get; set; }
        public IList<string> Thumbnails { get; set; }
        public string MainThumbnail { get; set; }
    }
}
