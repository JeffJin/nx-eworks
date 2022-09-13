using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace adworks.message_bus
{
    public static class SecurityUtils
    {
        public static JwtSecurityToken GetJwtToken(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                return null;
            }
            httpContext.Request.Headers.TryGetValue("Authorization", out StringValues tokenResults);
            if (tokenResults.Count != 1)
            {
                return null;
            }
            var handler = new JwtSecurityTokenHandler();
            string token = tokenResults.Single();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            string accessToken = token.Substring(7);
            
            return handler.ReadToken(accessToken) as JwtSecurityToken;
        }
    }
}