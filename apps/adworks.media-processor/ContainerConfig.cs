using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using adworks.data_services;
using adworks.media_common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace adworks.media_processor
{
    public static class ContainerConfig
    {
        public static ConfigResult Configure()
        {
            var services = new ServiceCollection();
            var configuration = AppConfig.GetConfig();

            services.AddAutofac();
            
            services.AddDbContext<CommonDbContext>();
           
            // Add Autofac
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<ProcessorModule>();
            containerBuilder.Populate(services);
            
            var container = containerBuilder.Build();
            
            IServiceProvider provider = new AutofacServiceProvider(container);
            return new ConfigResult()
            {
                Container = container,
                ServiceProvider = provider,
                Configuration = configuration
            };
        }
        
    }

    public class ConfigResult
    {
        public IContainer Container { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public IConfiguration Configuration { get; set; }
    }
}