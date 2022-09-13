using System;

namespace adworks.data_services
{
    public class AssetNotFoundException : Exception
    {
        public string AssetType { get; }
        public Guid AssetId { get; }

        public AssetNotFoundException(string assetType, Guid assetId)
        {
            AssetType = assetType;
            AssetId = assetId;
        }
    }
}