using System;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace adworks.pi_player
{
    public class ConfigResult
    {
        public IContainer Container { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public IConfiguration Configuration { get; set; }
    }
}