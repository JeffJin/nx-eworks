using System.IO;
using adworks.data_services;
using adworks.media_web_api.Attributes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;
using Twilio;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/sms")]
    public class SmsApiController : BaseApiController
    {
        public SmsApiController(IConfiguration configuration, IUserService userService,
            ILogger logger): base(configuration, userService, logger)
        {
        }

        // POST sms/inbound
        [HttpPost("inbound")]
        public void Inbound([FromBody] SMSMessage smsMessage)
        {
            _logger.Information("SmsController", smsMessage);
        }
        // POST sms/fallback
        [HttpPost("fallback")]
        public void Fallback(object smsError)
        {
            _logger.Information("SmsController", smsError);
        }
    }
}
