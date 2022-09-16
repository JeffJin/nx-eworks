using System;
using Newtonsoft.Json;

namespace adworks.media_common
{
    /// <summary>
    /// A generic object to represent a broadcasted event in our SignalR hubs
    /// </summary>
    public class SystemEvent
    {
        public SystemEvent()
        {
            Timestamp = DateTimeOffset.Now;
        }

        /// <summary>
        /// The source of the event
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// The message topic
        /// </summary>
        public string MessageTopic { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public Guid SourceId { get; set; }

        /// <summary>
        /// The date/time that the event was created
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }

    public class FtpEvent
    {
        public string SourceFile { get; set; }
        public string Destination { get; set; }
        public FtpEvent()
        {
            Timestamp = DateTimeOffset.Now;
        }
        public DateTimeOffset Timestamp { get; set; }

    }
}