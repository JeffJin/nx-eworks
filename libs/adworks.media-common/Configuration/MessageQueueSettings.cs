namespace adworks.media_common.Configuration
{
    public class MessageQueueSettings
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
        public string VirtualHost { get; set; }
    }
}