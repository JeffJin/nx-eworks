using System;

namespace adworks.media_common
{
    public class MessageIdentity
    {

        public MessageIdentity(string organization, string group, string identity)
        {
            if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(identity))
            {
                throw new ArgumentException("organization or id cannot be null or empty");
            }
            Organization = organization;
            Group = group;
            Identity = identity;
        }

        public string Organization { get; } // Logged in user ID
        public string Group { get; } // Logged in user ID, Device Group, etc.
        public string Identity { get; } //browser ID, device serial number, etc.

        public override string ToString()
        {
            return $"{Organization}.{Group}.{Identity}";
        }

        public override bool Equals(object? obj)
        {
            var target = obj as MessageIdentity;
            if (target == null)
            {
                return false;
            }

            return Equals(target);
        }

        private bool Equals(MessageIdentity other)
        {
            return Organization == other.Organization && Group == other.Group && Identity == other.Identity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Organization, Group, Identity);
        }
    }
}