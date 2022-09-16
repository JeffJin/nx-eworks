using System;
using System.Collections.Specialized;
using adworks.media_common;
using adworks.pi_player.Scheduling;
using adworks.pi_player.Scheduling.Jobs;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.Quartz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace adworks.pi_player
{
    public static class ContainerConfig
    {
        public static ConfigResult Configure()
        {
            var services = new ServiceCollection();
            var configuration = AppConfig.GetConfig();

            services.AddAutofac();
            
            // Add Autofac
            var containerBuilder = new ContainerBuilder();

            var quartzConnection = configuration.GetConnectionString("QuartznetConnection");
            
            //configure quartz.net
            var schedulerConfig = new NameValueCollection {
                {"quartz.scheduler.instanceName", "PlaylistScheduler"},
                {"quartz.scheduler.instanceId", "Quartz"},
                {"quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"},
                {"quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz"},
                {"quartz.jobStore.lockHandler.type", "Quartz.Impl.AdoJobStore.UpdateLockRowSemaphore, Quartz"},
                {"quartz.jobStore.tablePrefix", "QRTZ_"},
                {"quartz.jobStore.dataSource", "default"},
                {"quartz.dataSource.default.connectionString", quartzConnection},
                {"quartz.dataSource.default.provider", "MySql"},
                {"quartz.jobStore.useProperties", "false"},
                {"quartz.threadPool.threadCount", "3"},
                {"quartz.serializer.type", "binary"}
            };
            
            containerBuilder.RegisterModule(new QuartzAutofacFactoryModule {
                ConfigurationProvider = c => schedulerConfig
            });
            containerBuilder.RegisterModule(new QuartzAutofacJobsModule(typeof(ReportStatusJob).Assembly));
            
            //register pi services
            containerBuilder.RegisterModule<PiModule>();

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
}