using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using ILogger = Serilog.ILogger;


namespace adworks.networking
{
    public class EmailService : IEmailService
    {
        private readonly ILogger _logger;
        private readonly string serverAddress;
        private readonly string account;
        private readonly string password;
        private readonly int port;

        public EmailService(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            var smptSettings = configuration.GetSection("SMTP").Get<SmptSettings>();
            serverAddress = smptSettings.Server;
            account = smptSettings.Account;
            password = smptSettings.Password;
            port = smptSettings.Port;
        }

        public EmailService(ILogger logger, string server, string account, string password, int port)
        {
            _logger = logger;
            serverAddress = server;
            account = account;
            password = password;
            port = port;
        }

        public async Task SendRegistrationEmail(string to, string callbackUrl)
        {
            SmtpClient smtpClient = GetSmtpClient();

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(account);
            mail.To.Add(to);
            mail.Subject = "Registration";
            mail.IsBodyHtml = true;

            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var templateFile = Path.Combine(basePath, "Assets", "registration.html");
            var imageFolder = Path.Combine(basePath, "Assets", "images");
            var payloads = new Dictionary<string, Stream>();
            using (var stream = File.OpenText(templateFile))
            {
                mail.Body = stream.ReadToEnd();
            }

            mail.Body = mail.Body.Replace("CALLBACK_URL", callbackUrl);

            var streamsToDispose = new List<Stream>();
            var images = Directory.GetFiles(imageFolder);
            foreach (var image in images)
            {
                var imageStream = File.Open(image, FileMode.Open);
                payloads.Add(Path.GetFileName(image), imageStream);
                streamsToDispose.Add(imageStream);
            }

            mail = ProcessMailMessage(mail, payloads);
            try
            {
                await smtpClient.SendMailAsync(mail);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to send email");
            }
            finally
            {
                try
                {
                    streamsToDispose.ForEach(s => s.Dispose());
                }
                catch
                {
                }
            }
        }

        public async Task Send(string to, string subject, string htmlBody, Dictionary<string, Stream> payload)
        {
            SmtpClient smtpClient = GetSmtpClient();

            var mail = new MailMessage();
            mail.From = new MailAddress(account);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.IsBodyHtml = true;
            mail.Body = htmlBody;
            mail = ProcessMailMessage(mail, payload);
            try
            {
                await smtpClient.SendMailAsync(mail);
            }
            catch (Exception t)
            {
                _logger.Error(t, "Unable to send email");
            }
        }

        public Task SendForgotPasswordEmail(string email, string callbackUrl)
        {
            throw new NotImplementedException();
        }

        public Task SendVerificationEmail(string userEmail, string code)
        {
            throw new NotImplementedException();
        }

        public Task SendNotificationEmail(string appointmentEmail)
        {
            throw new NotImplementedException();
        }

        private MailMessage ProcessMailMessage(MailMessage mail, Dictionary<string, Stream> payload)
        {
            AlternateView plainView = AlternateView.CreateAlternateViewFromString("Please view as HTML-Mail.", System.Text.Encoding.UTF8, "text/plain");
            plainView.TransferEncoding = TransferEncoding.QuotedPrintable;

            AlternateView htmlView =
                AlternateView.CreateAlternateViewFromString(mail.Body, Encoding.UTF8, MediaTypeNames.Text.Html);
            foreach (var item in payload)
            {
                LinkedResource imageResource = new LinkedResource(item.Value, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = item.Key,
                    TransferEncoding = TransferEncoding.QuotedPrintable,
                    ContentType =
                    {
                        Name = item.Key,
                        MediaType = MediaTypeNames.Image.Jpeg
                    },
                    ContentLink = new Uri($"cid:{item.Key}")
                };
                htmlView.LinkedResources.Add(imageResource);
            }
            mail.AlternateViews.Add(plainView);
            mail.AlternateViews.Add(htmlView);

            return mail;
        }


        public SmtpClient GetSmtpClient()
        {
            return GetSmtpClient(serverAddress, port, account, password);
        }

        private SmtpClient GetSmtpClient(string server, int port, string user, string password)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = server;
            smtpClient.Port = port;
            NetworkCredential cred = new NetworkCredential(user, password);
            smtpClient.EnableSsl = true;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Timeout = 5000;
            smtpClient.Credentials = cred;
            return smtpClient;
        }
    }
}
