using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using adworks.media_web_api.Services;
using adworks.networking;

namespace adworks.media_web_api.Services
{
    public static class EmailSenderExtensions
    {
        public static async Task SendEmailConfirmationAsync(this IEmailService emailSender, string email, string link)
        {
            await emailSender.SendEmailConfirmationAsync(email, link);
        }
    }
}
