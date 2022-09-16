using System;
using System.ComponentModel.DataAnnotations;

namespace adworks.data_services.DbModels
{
    /// <summary>
    /// Asset merge record
    /// For example, audio + image, video + audio, video + video(PiP), video + image(PiP)
    /// </summary>
    public class MergeRecord: Entity
    {
        [Required]
        public string MergeType { get; set; } //I+A, V+A, V+V, V+I
        [Required]
        public Guid AssetId1 { get; set; }
        [Required]
        public Guid AssetId2 { get; set; }
    }
}