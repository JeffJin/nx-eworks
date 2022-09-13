using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace adworks.networking
{
    public interface IEmailService
    {
        Task SendRegistrationEmail(string to, string callbackUrl);

        Task Send(string to, string subject, string htmlBody, Dictionary<string, Stream> payload);
        Task SendForgotPasswordEmail(string email, string callbackUrl);
        Task SendVerificationEmail(string userEmail, string code);
        Task SendNotificationEmail(string appointmentEmail);
    }
}
