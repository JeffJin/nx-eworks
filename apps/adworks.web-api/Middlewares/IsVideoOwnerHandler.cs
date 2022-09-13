using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.Identity;
using adworks.media_common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace adworks.media_web_api.Middlewares
{
    public class IsVideoOwnerHandler : AuthorizationHandler<IsVideoOwnerRequirement, VideoDto>
    {
        private readonly UserManager<User> _userManager;

        public IsVideoOwnerHandler(
            UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IsVideoOwnerRequirement requirement,
            VideoDto resource)
        {
            var appUser = await _userManager.GetUserAsync(context.User);
            if (appUser == null)
            {
                return;
            }

            if (resource.CreatedBy == appUser.Email)
            {
                context.Succeed(requirement);
            }
        }
    }
}