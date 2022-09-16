using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace adworks.media_common.Extensions
{
    public static class TokenExtensions
    {
        public static string ToTokenString(this JwtSecurityToken token)
        {
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GetClaim(this JwtSecurityToken token, string claimType)
        {
            return token?.Claims?.FirstOrDefault(c => c.Type == claimType)?.Value;
        }

        public static string GetEmail(this JwtSecurityToken token)
        {
            return GetClaim(token, ClaimTypes.Email);
        }

        public static string GetName(this JwtSecurityToken token)
        {
            return GetClaim(token, ClaimTypes.Name);
        }

        public static string GetId(this JwtSecurityToken token)
        {
            return GetClaim(token, ClaimTypes.NameIdentifier);
        }

        public static string GetJti(this JwtSecurityToken token)
        {
            return GetClaim(token, JwtRegisteredClaimNames.Jti);
        }

        public static string GetOrganization(this JwtSecurityToken token)
        {
            return GetClaim(token, TokenClaimTypes.Organization);
        }

        public static string GetGroup(this JwtSecurityToken token)
        {
            return GetClaim(token, TokenClaimTypes.Group);
        }
    }
}