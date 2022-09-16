using System;
using System.Security;
using System.Threading.Tasks;
using adworks.data_services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using adworks.data_services.Identity;
using adworks.media_common;
using adworks.media_common.Extensions;
using adworks.media_web_api.Middlewares;
using adworks.message_bus;
using adworks.networking;
using Facebook;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/account")]
    public class AccountApiController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IOrganizationService _organizationService;
        private readonly IRabbitListener _rabbitListener;
        private readonly string _webClient;

        private string _fbAppAccessToken = "";
        public AccountApiController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailService emailService,
            ISmsService smsService,
            IConfiguration configuration,
            IUserService userService,
            IOrganizationService organizationService,
            IRabbitListener rabbitListener,
            ILogger logger): base(configuration, userService, logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _smsService = smsService;
            _organizationService = organizationService;
            _rabbitListener = rabbitListener;
            _fbAppAccessToken = GetFbAppAccessToken();
            _webClient = _configuration["WebClient"];
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<LoginResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, false, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var appUser = await _userService.GetUserByEmail(dto.Email);
                    var token = TokenHelper.GenerateToken(appUser, _configuration["JwtAudience"], _configuration["JwtIssuer"],
                        _configuration["JwtSecretKey"], Convert.ToInt16(_configuration["JwtExpireMinutes"]));

                    //TODO store the token into Redis server
                    //https://stackoverflow.com/questions/52476325/how-to-invalidate-tokens-after-password-change

                    _rabbitListener.Register(token.GetOrganization(), token.GetId(), token.GetJti());

                    return new LoginResult(token.ToTokenString(), appUser.UserName, appUser.Email, appUser.PhoneNumber);
                }

                throw new UnauthorizedException(dto.Email);
            }
            catch (Exception e)
            {
                throw new UnauthorizedException(dto.Email);
            }
        }

        private string GetFbAppAccessToken()
        {
            var fb = new FacebookClient();
            JsonObject tokenResult = (JsonObject)fb.Get("oauth/access_token", new {
                client_id     = _configuration["Facebook:AppID"],
                client_secret = _configuration["Facebook:AppSecret"],
                grant_type    = "client_credentials"
            });
            object tokenValue = null;
            tokenResult.TryGetValue("access_token", out tokenValue);
            return tokenValue as string;
        }

        [HttpPost("fb_login")]
        [AllowAnonymous]
        public async Task<LoginResult> FbLogin([FromBody] FbLoginDto dto)
        {
            try
            {
                // step 1: validate fb token against fb authenticator
                var fb = new FacebookClient(_fbAppAccessToken);
                JsonObject validationResult = (JsonObject)fb.Get($"debug_token?input_token={dto.AccessToken}");

                object data = null;
                validationResult.TryGetValue("data", out data);
                object isValid = null;
                ((JsonObject)data)?.TryGetValue("is_valid", out isValid);
                if (isValid != null && !((bool)isValid))
                {
                    throw new UnauthorizedException(dto.Email);
                }
                // step 2: find the user and create or update and then signin
                return await ExternalSignIn(dto.Email, dto.UserName);
            }
            catch (Exception e)
            {
                throw new UnauthorizedException(dto.Email, e);
            }
        }

        private async Task<LoginResult> ExternalSignIn(string email, string userName)
        {
            var appUser = await _userService.GetUserByEmail(email);
            if (appUser == null)
            {
                var user = new User {UserName = userName.Replace(" ", ""), Email = email};
                var result = await _userManager.CreateAsync(user);

                if (result is not {Succeeded: true})
                {
                    throw new UnauthorizedException(email);
                }
                appUser = await _userService.GetUserByEmail(email);
            }

            if (appUser.Organization == null)
            {
                await _organizationService.AddUser(DtoHelper.DefaultOrganization, email);
                appUser = await _userService.GetUserByEmail(email);
            }

            var token = TokenHelper.GenerateToken(appUser.Id, appUser.Email, appUser.UserName, appUser.Organization.Name,
                _configuration["JwtAudience"],
                _configuration["JwtIssuer"],
                _configuration["JwtSecretKey"],
                Convert.ToInt16(_configuration["JwtExpireMinutes"]));
            //TODO remove the token from Redis server

            _rabbitListener.Register(token.GetOrganization(), token.GetId(), token.GetJti());

            return new LoginResult(token.ToTokenString(), userName, email, "");
        }


        [HttpPost("login_with_2fa")]
        [AllowAnonymous]
        public async Task<SignInResult> LoginWith2fa(string twoFactorCode, string provider, bool rememberMachine, bool rememberMe)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = twoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorSignInAsync(provider, authenticatorCode, rememberMe,
                rememberMachine);

            var token = TokenHelper.GenerateToken(user, _configuration["JwtAudience"], _configuration["JwtIssuer"],
                _configuration["JwtSecretKey"], Convert.ToInt16(_configuration["JwtExpireMinutes"]));

            _rabbitListener.Register(token.GetOrganization(), token.GetId(), token.GetJti());

            return result;
        }

        [HttpGet("get_2fa_user")]
        [AllowAnonymous]
        public async Task<User> GetTwoFactorAuthenticationUser()
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            return user;
        }

        [HttpGet("send_verification_code")]
        [AllowAnonymous]
        public async Task SendVerificationCode(string provider)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var code = await _userManager.GenerateTwoFactorTokenAsync(user, provider);
            if (provider == "Phone")
            {
                _smsService.Send(user.PhoneNumber, code);
            }
            else if(provider == "Email")
            {
                await _emailService.SendVerificationEmail(user.Email, code);
            }
        }

        [HttpPost("login_with_recovery_code")]
        [AllowAnonymous]
        public async Task<SignInResult> LoginWithRecoveryCode(LoginWithRecoveryCodeDto model)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);
            var token = TokenHelper.GenerateToken(user, _configuration["JwtAudience"], _configuration["JwtIssuer"],
                _configuration["JwtSecretKey"], Convert.ToInt16(_configuration["JwtExpireMinutes"]));

            _rabbitListener.Register(token.GetOrganization(), token.GetId(), token.GetJti());
            return result;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IdentityResult> Register(RegisterDto model)
        {
            var user = new User {UserName = model.Email, Email = model.Email};
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.Information($"User created a new account with user name {0}", model.Email);
                string orgName = string.IsNullOrEmpty(model.OrganizationName) ? DtoHelper.DefaultOrganization : model.OrganizationName;
                await _organizationService.AddUser(orgName, model.Email);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.EmailConfirmationUrl(user.Id.ToString(), token, _webClient);
                await _emailService.SendRegistrationEmail(model.Email, callbackUrl);
            }

            return result;
        }

        [HttpPost("logout")]
        public async Task<LogoutDto> Logout()
        {
            _rabbitListener.Deregister();

            await _signInManager.SignOutAsync();
            _logger.Information("User logged out.");
            return new LogoutDto()
            {
                Message = "Success"
            };
        }

        [HttpPost("confirm_email")]
        [AllowAnonymous]
        public async Task<IdentityResult> ConfirmEmail(ConfirmEmailDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{dto.UserId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, dto.Code);
            return result;
        }


        [HttpPost("forgot_password")]
        [AllowAnonymous]
        public async Task ForgotPassword(ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                //TODO Don't reveal that the user does not exist or is not confirmed
                return;
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.ResetPasswordCallbackUrl(user.Id.ToString(), code, _webClient);
            await _emailService.SendForgotPasswordEmail(model.Email, callbackUrl);

        }

        [HttpPost("reset_password")]
        [AllowAnonymous]
        public async Task<IdentityResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return null;
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            return result;
        }
    }
}
