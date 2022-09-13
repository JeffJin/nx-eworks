using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace adworks.media_common.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        public static string GetClaim(this ClaimsPrincipal user, string claimType)
        {
            return user?.Claims?.FirstOrDefault(o => o.Type == claimType)?.Value;
        }
        
        public static string GetEmail(this ClaimsPrincipal user)
        {
            return GetClaim(user, ClaimTypes.Email);
        }

        public static string GetId(this ClaimsPrincipal user)
        {
            return GetClaim(user, ClaimTypes.NameIdentifier);
        }

        public static string GetName(this ClaimsPrincipal user)
        {
            return GetClaim(user, ClaimTypes.Name);
        }

        public static string GetJti(this ClaimsPrincipal user)
        {
            return GetClaim(user, JwtRegisteredClaimNames.Jti);
        }

        public static string GetOrganization(this ClaimsPrincipal user)
        {
            return GetClaim(user, TokenClaimTypes.Organization);
        }

        public static string GetGroup(this ClaimsPrincipal user)
        {
            return GetClaim(user, TokenClaimTypes.Group);
        }
    }
}