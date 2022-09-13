using System.ComponentModel.DataAnnotations;

namespace adworks.media_common
{
    public class AudioDto: MediaAssetDto
    {
        public AudioDto()
        {
        }
        public AudioDto(MediaAssetDto assetDto)
        {
            RawFilePath = assetDto.RawFilePath;
            FileSize = assetDto.FileSize;
            Category = assetDto.Category;
            Title = assetDto.Title;
            Description = assetDto.Description;
            Tags = assetDto.Tags;
            CloudUrl = assetDto.CloudUrl  ?? assetDto.RawFilePath;
            Id = assetDto.Id;
            UpdatedOn = assetDto.UpdatedOn;
            UpdatedBy = assetDto.UpdatedBy;
            CreatedBy = assetDto.CreatedBy;
        }
        public string EncodedFilePath { get; set; }
        public double Duration { get; set; }
    }
}