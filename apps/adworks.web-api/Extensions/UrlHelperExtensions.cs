using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using adworks.media_web_api.Controllers;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationUrl(this IUrlHelper urlHelper, string userId, string code, string webClient)
        {
            string codeHtmlVersion = HttpUtility.UrlEncode(code);
            return $"{webClient}/confirm_email?userId={userId}&code={codeHtmlVersion}";
        }

        public static string ResetPasswordCallbackUrl(this IUrlHelper urlHelper, string userId, string code, string webClient)
        {
            return $"{webClient}/reset_password?uid={userId}&code={code}";
        }
        
        public static string LinkLoginCallbackUrl(this IUrlHelper urlHelper, string webClient)
        {
            return $"{webClient}/link_login_callback";
        }
        
        
    }
}
