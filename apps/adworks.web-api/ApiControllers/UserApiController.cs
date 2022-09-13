using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using adworks.data_services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ILogger = Serilog.ILogger;
using adworks.data_services.Identity;
using adworks.media_web_api.Middlewares;
using adworks.media_common;
using adworks.media_common.Extensions;
using adworks.message_bus;
using adworks.networking;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/users")]
    public class UserApiController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly UrlEncoder _urlEncoder;
        private readonly IRabbitListener _rabbitListener;
        private readonly string _webClient;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        private static Regex userNameRegex = new Regex(@"(\.|\-|\._|\-_)$", RegexOptions.Compiled);
        private static Regex emailRegex = new Regex(@"(\.|\-|\._|\-_)$", RegexOptions.Compiled);
        public UserApiController(UserManager<User> userManager, IConfiguration configuration,
            SignInManager<User> signInManager, IEmailService emailService, IUserService userService, UrlEncoder urlEncoder,
            IRabbitListener rabbitListener, ILogger logger): base(configuration, userService, logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _urlEncoder = urlEncoder;
            _rabbitListener = rabbitListener;
            _webClient = _configuration["WebClient"];
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet("current")]
        public async Task<UserDto> GetUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var appUser = await _userService.GetUserByEmail(user.Email);
            var token = TokenHelper.GenerateToken(appUser, _configuration["JwtAudience"], _configuration["JwtIssuer"],
                _configuration["JwtSecretKey"], Convert.ToInt16(_configuration["JwtExpireMinutes"]));
            _rabbitListener.Register(token.GetOrganization(), token.GetId(), token.GetJti());

            return DtoHelper.Convert(user);
        }

        [HttpGet("validate_email")]
        [AllowAnonymous]
        public async Task<KeyValuePair<string, bool>?> ValidateEmail(string email)
        {
            if (!ValidationUtilities.IsValidEmail(email))
            {
                return new KeyValuePair<string, bool>("InvalidEmail", false);

            }
            var result = await _userService.ValidateEmail(email);
            return result ? new KeyValuePair<string, bool>("DuplicatedEmail", true) : null;
        }

        [HttpGet("validate_username")]
        [AllowAnonymous]
        public async Task<KeyValuePair<string, bool>?> ValidateUserName(string userName)
        {
            if (!ValidationUtilities.IsValidUserName(userName))
            {
                return new KeyValuePair<string, bool>("InvalidUserName", false);
            }
            var result = await _userService.ValidateUserName(userName);
            return result ? new KeyValuePair<string, bool>("DuplicatedUserName", true) : null;
        }

        [HttpPut("update")]
        public async Task<User> UpdateUser([FromBody] UserDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            var phoneNumber = user.PhoneNumber;
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }

            return user;
        }

        [HttpPost("send_verification_email")]
        public async Task SendVerificationEmail(UserDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.EmailConfirmationUrl(user.Id.ToString(), code, _webClient);
            var email = user.Email;
            await _emailService.SendVerificationEmail(email, callbackUrl);
        }

        [HttpPost("change_password")]
        public async Task<IdentityResult> ChangePassword(ChangePasswordDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                throw new ApplicationException($"Unable to change password for user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.Information("User changed their password successfully.");

            return changePasswordResult;
        }


        [HttpPost("set_password")]
        public async Task<IdentityResult> SetPassword(SetPasswordDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                throw new ApplicationException($"Unable to set password for user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return addPasswordResult;
        }

        [HttpGet("external_logins")]
        public async Task<ExternalLoginsDto> ExternalLogins()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new ExternalLoginsDto { CurrentLogins = await _userManager.GetLoginsAsync(user) };
            model.OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            model.ShowRemoveButton = await _userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
            model.StatusMessage = StatusMessage;

            return model;
        }

        [HttpPost("link_login")]
        public async Task<ChallengeDto> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.LinkLoginCallbackUrl(_webClient);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeDto(provider, properties);
        }

        [HttpGet("link_login_callback")]
        public async Task<IdentityResult> LinkLoginCallback()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(user.Id.ToString());
            if (info == null)
            {
                throw new NotFoundException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
            }

            var result = await _userManager.AddLoginAsync(user, info);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return result;
        }

        [HttpPost("remove_login")]
        public async Task<IdentityResult> RemoveLogin(RemoveLoginDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var result = await _userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred removing external login for user with ID '{user.Id}'.");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return result;
        }

        [HttpGet("two_factor_authentication")]
        public async Task<TwoFactorAuthenticationDto> TwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new TwoFactorAuthenticationDto
            {
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = user.TwoFactorEnabled,
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            };

            return model;
        }

        [HttpPost("disable_2fa")]
        public async Task<IdentityResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2FaResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2FaResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
            }

            _logger.Information("User with ID {UserId} has disabled 2fa.", user.Id);
            return disable2FaResult;
        }

        [HttpGet("get_authenticator")]
        public async Task<EnableAuthenticatorDto> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var model = new EnableAuthenticatorDto
            {
                SharedKey = FormatKey(unformattedKey),
                AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey)
            };

            return model;
        }

        [HttpPost("enable_authenticator")]
        public async Task<IdentityResult> EnableAuthenticator(EnableAuthenticatorDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                throw new ApplicationException("Verification code is invalid.");
            }

            var identityResult = await _userManager.SetTwoFactorEnabledAsync(user, true);
            _logger.Information("User with ID {UserId} has enabled 2FA with an authenticator app.", user.Id);
            return identityResult;
        }

        [HttpPost("reset_authenticator")]
        public async Task<IdentityResult> ResetAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            var result = await _userManager.ResetAuthenticatorKeyAsync(user);
            _logger.Information("User with id '{UserId}' has reset their authentication app key.", user.Id);

            return result;
        }

        [HttpGet("get_recovery_codes")]
        public async Task<GenerateRecoveryCodesDto> GenerateRecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new NotFoundException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!user.TwoFactorEnabled)
            {
                throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            var model = new GenerateRecoveryCodesDto { RecoveryCodes = recoveryCodes.ToArray() };

            _logger.Information("User with ID {UserId} has generated new 2FA recovery codes.", user.Id);

            return model;
        }

        #region Helpers

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("adworks.media_web_api"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        #endregion
    }
}
