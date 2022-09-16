namespace adworks.pi_player.Services
{
    public class ProgressPayload
    {
        public long TotalSize { get; set; }
        public long DownloadedSize { get; set; }
        public string AssetUrl { get; set; }
        public string AssetTitle { get; set; }

        public ProgressPayload()
        {
            
        }
    }
}