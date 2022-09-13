using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace adworks.message_bus
{
    public interface IRabbitListener
    {
        void Register(string organization, string user, string sessionId, string group = "*");
        void Register();
        void Deregister(string organization, string user, string sessionId, string group = "*");
        void Deregister();
    }
}