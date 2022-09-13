namespace adworks.message_common
{
    public static class MessageTopics
    {
        //Videos
        public const string VideoUploaded = "VideoUploaded";
        public const string VideoEncode = "VideoEncode";
        public const string VideoStartProcessing = "VideoStartProcessing";
        public const string VideoFinishProcessing = "VideoFinishedProcessing";
        public const string VideoFailedProcessing = "VideoFailedProcessing";
        public const string VideoAddText = "VideoAddText";
        public const string VideoAddTextResult = "VideoAddTextResult";

        //Audios
        public const string ImageUploaded = "ImageUploaded";
        public const string ImageStartProcessing = "ImageStartProcessing";
        public const string ImageFinishProcessing = "ImageFinishProcessing";
        public const string ImageFailedProcessing = "ImageFailedProcessing";
        public const string ImageMergeAudio = "ImageMergeAudio";
        public const string ImageMergeAudioFinished = "ImageMergeAudioFinished";

        //Images
        public const string AudioUploaded = "AudioUploaded";
        public const string AudioEncode = "AudioEncode";
        public const string AudioStartProcessing = "AudioStartProcessing";
        public const string AudioFinishProcessing = "AudioFinishProcessing";
        public const string AudioFailedProcessing = "AudioFailedProcessing";
        
        //Pi client
        public const string PiDownloadAssetProgress = "PiDownloadAssetProgress";
        public const string PiReportStatus = "PiReportStatus";
        public const string PiPlaylistPublished = "PiPlaylistPublished";
        public const string PiPlayNextVideo = "PiPlayNextVideo";
        public const string PiPlaylistUnscheduled = "PiPlaylistUnscheduled";
        
        //General
        public const string TemplateTransferToFtpSiteProgress = "{0}TransferToFtpSiteProgress";
        public const string TemplateTransferToFtpSite = "{0}TransferToFtpSite";
        public const string TemplateTransferToFtpSiteComplete = "{0}TransferToFtpSiteComplete";
    }
}