using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.media_common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_web_api.Controllers
{
    public abstract class BaseApiController : Controller
    {
        protected readonly IUserService _userService;

        protected readonly ILogger _logger;
        protected IConfiguration _configuration { get; }

        protected BaseApiController(IConfiguration configuration, IUserService userService, ILogger logger)
        {
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        protected Claim TryGetClaim(ClaimsPrincipal owinUser, string key)
        {
            return owinUser?.Claims?.FirstOrDefault(o => o.Type.Equals(key));
        }


        protected async Task<MessageIdentity> GetUserIdentity()
        {
            var claim= TryGetClaim(HttpContext.User , ClaimTypes.Email);
            string email = claim.Value;
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var user = await _userService.GetUserByEmail(email);
            return new MessageIdentity(user.Organization.Name, "*", user.Id);
        }

        protected UserDto GetCurrentUser()
        {
            var claim= TryGetClaim(HttpContext.User , ClaimTypes.Email);
            string email = claim.Value;
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }
            return new UserDto(email);
        }

        protected string GetCurrentEmail()
        {
            var claim= TryGetClaim(HttpContext.User , ClaimTypes.Email);
            string email = (claim == null || string.IsNullOrEmpty(claim.Value)) ? _configuration["DefaultUser"] : claim.Value;
            return email;
        }

    }
}
