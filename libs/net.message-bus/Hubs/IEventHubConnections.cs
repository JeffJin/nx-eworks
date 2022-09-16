using System.Collections.Generic;

namespace adworks.message_bus
{
    public interface IEventHubConnections
    {
        IEnumerable<string> GetConnections(string userId);
    }
}