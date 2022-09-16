namespace adworks.media_common.Configuration
{
    public class FtpSettings
    {
        public string ProcessorAddress { get; set; }
        public string StreamingAddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VideoFolder { get; set; }
        public string ImageFolder { get; set; }
        public string AudioFolder { get; set; }
        public bool? Ssl { get; set; }
    }
}