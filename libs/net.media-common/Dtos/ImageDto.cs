namespace adworks.media_common
{
    public class ImageDto: MediaAssetDto
    {
        public ImageDto()
        {
        }
        public ImageDto(MediaAssetDto assetDto)
        {
            RawFilePath = assetDto.RawFilePath;
            FileSize = assetDto.FileSize;
            Category = assetDto.Category;
            Title = assetDto.Title;
            Description = assetDto.Description;
            Tags = assetDto.Tags;
            CloudUrl = assetDto.CloudUrl ?? assetDto.RawFilePath;
            Id = assetDto.Id;
            UpdatedOn = assetDto.UpdatedOn;
            UpdatedBy = assetDto.UpdatedBy;
            CreatedBy = assetDto.CreatedBy;
        }
        //check if it is animated GIF image
        public string ImageType { get; set; }
    }
}