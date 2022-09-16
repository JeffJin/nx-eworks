using System;

namespace adworks.data_services
{
    public class InvalidAssetTypeException : Exception
    {
        public string AssetType { get; }

        public InvalidAssetTypeException(string assetType)
        {
            AssetType = assetType;
        }
    }
}