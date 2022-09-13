using System;
 using adworks.media_common;
 
 namespace adworks.media_common
 {
     [Serializable]
     public class PlaylistItemDto: EntityDto
     {
         public Guid SubPlaylistId { get; set; }
         public Guid MediaAssetId { get; set; }
         public string AssetDiscriminator { get; set; }
         public int Duration { get; set; }
         public int Index { get; set; }
         public MediaAssetDto Media { get; set; }
         public string CacheLocation { get; set; }
     }
 }