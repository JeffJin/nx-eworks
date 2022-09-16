using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace adworks.networking
{
    public class SmsService: ISmsService
    {
        private readonly ILogger _logger;
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;
        public SmsService(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _accountSid = configuration["Sms:AccountSid"];
            _authToken = configuration["Sms:AuthToken"];
            _fromNumber = configuration["Sms:FromNumber"];
        }

        public string Send(string phone, string text)
        {
            try
            {
                TwilioClient.Init(_accountSid, _authToken);
                var to = new PhoneNumber(phone);
                var message = MessageResource.Create(
                    to,
                    from: new PhoneNumber(_fromNumber), //  From number, must be an SMS-enabled Twilio number ( This will send sms from ur "To" numbers ).
                    body: text);
                return message.Status.ToString();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to send sms message");
                throw;
            }
        }
    }
}
