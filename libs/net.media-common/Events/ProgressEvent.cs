using System;
using Newtonsoft.Json;

namespace adworks.media_common
{
    /// <summary>
    /// A generic object to represent a broadcasted event in our SignalR hubs
    /// </summary>
    public class ProgressEvent
    {
        public ProgressEvent()
        {
            Timestamp = DateTimeOffset.Now;
        }

        /// <summary>
        /// The name of the event
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// The name of the channel the event is associated with
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// The date/time that the event was created
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}