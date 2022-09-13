using System;

namespace adworks.media_common
{
    public class LoginResult
    {
        public LoginResult(bool invalidLogin)
        {
            InvalidLogin = invalidLogin;
        }
        public LoginResult(string email, bool requiresTwoFactor = false, bool isLockedOut = false)
        {
            Email = email;
            RequiresTwoFactor = requiresTwoFactor;
            IsLockedOut = isLockedOut;
        }
        
        public LoginResult(string token, string userName, string email, string phoneNumber)
        {
            Token = token;
            UserName = userName;
            Email = email;
            PhoneNumber = phoneNumber;
        }
        public string Token { get; set; }
        public string UserName { get; set;  }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        
        public bool RequiresTwoFactor { get; set; }
        public bool IsLockedOut { get; set; }
        public bool InvalidLogin { get; set; }
    }
}