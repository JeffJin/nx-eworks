using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace adworks.media_common
{
    public static class AppConfig
    {
        public static IConfiguration GetConfig(string env = "")
        {
            string config;
            var tempEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(env))
            {
                env = tempEnv;
            }

            if (string.IsNullOrEmpty(env) && string.IsNullOrEmpty(tempEnv))
            {
                config = $"appsettings.json";
            }
            else
            {
                config = $"appsettings.{env}.json";
            }
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(config, optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
            var configuration = configBuilder.Build();

            return configuration;
        }
    }
}
