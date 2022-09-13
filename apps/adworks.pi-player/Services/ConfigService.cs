using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using adworks.media_common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player.Services
{
    public class ConfigService: IConfigService
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;


        public ConfigService(IConfiguration config,ILogger logger)
        {
            _config = config;
            _logger = logger;
        }



    }
}
