using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using adworks.data_services;
using adworks.data_services.DbModels;
using adworks.media_common;
using adworks.message_bus;
using adworks.networking;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_web_api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/error")]
    public class ErrorApiController : BaseApiController
    {
        private readonly IFileService _fileService;
        private readonly IFtpService _ftpService;

        // Get the default form options so that we can use them to set the default limits for
        // request body data

        public ErrorApiController(IFileService fileService, IConfiguration configuration, IUserService userService,
            IFtpService ftpService, ILogger logger): base(configuration, userService, logger)
        {
            _fileService = fileService;
            _ftpService = ftpService;
        }

        // GET api/audio/list
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new List<string>()
            {
                "error 1",
                "error 2"
            };
        }

        // POST api/error
        [HttpPost("publish/{id}")]
        public bool Post(string error)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to update audio", ex);
                throw;
            }

        }
    }
}
