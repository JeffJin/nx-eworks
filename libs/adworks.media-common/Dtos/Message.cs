using System;

namespace adworks.media_common
{
    public class Message
    {      
        public Message(MessageIdentity messageIdentity)
        {
            MessageIdentity = messageIdentity;
        }
        
        /// A key field that uniquely identifies an entity
        /// a combination of organization name, group name and user email, device serial number
        ///
        /// routing key format: Organization.Group.Identity
        /// example 1: eworkspace.basement.J889900KLL777.ReportStatus
        /// example 2: eworkspace.30dd879c-ee2f-11db-8314-0800200c9a66.123458437584932758943.VideoPlayed
       
        public MessageIdentity MessageIdentity { get; }
        public string Topic { get; set; }
        public string Body { get; set; }

        public override string ToString()
        {
            return MessageIdentity.ToString() + ", " + Topic + ", " + Body;
        }
    }
}
 