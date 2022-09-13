using System;
using System.IO;
using adworks.media_common;
using adworks.message_bus;
using adworks.networking;
using adworks.pi_player.Processors;
using adworks.pi_player.Scheduling;
using adworks.pi_player.Services;
using Autofac;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
using ILogger = Serilog.ILogger;
using Serilog.Events;
using Serilog.Exceptions;

namespace adworks.pi_player
{
    public class PiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var configuration = AppConfig.GetConfig();

            builder.RegisterType<MessageClient>().As<IMessageClient>().SingleInstance();
            builder.RegisterType<PlaylistProcessor>().As<IPlaylistProcessor>().SingleInstance();
            builder.RegisterType<AuditProcessor>().As<IAuditProcessor>().SingleInstance();

            builder.Register<ILogger>((c, p) =>
            {
                var logger =  new LoggerConfiguration()
                    .Enrich.WithExceptionDetails()
                    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}",
                        restrictedToMinimumLevel:LogEventLevel.Debug)
                    .WriteTo.RollingFile(Path.Combine(configuration["BaseAssetFolder"], configuration["LogFile"]),
                        LogEventLevel.Information)
                    .CreateLogger();

                Log.Logger = logger;
                return logger;

            }).SingleInstance();

            builder.RegisterType<DataService>().As<IDataService>().InstancePerLifetimeScope();
            builder.RegisterType<ShellService>().As<IShellService>().InstancePerLifetimeScope();
            builder.RegisterType<SystemService>().As<ISystemService>().InstancePerLifetimeScope();
            builder.RegisterType<SchedulerService>().As<ISchedulerService>().InstancePerLifetimeScope();
            builder.RegisterType<DownloadService>().As<IDownloadService>().InstancePerLifetimeScope();
            builder.RegisterType<CachingService>().As<ICachingService>().InstancePerLifetimeScope();
            builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<SmsService>().As<ISmsService>().InstancePerLifetimeScope();
            builder.RegisterType<PlaylistScheduler>().As<IPlaylistScheduler>().InstancePerLifetimeScope();
            builder.RegisterType<ReportStatusScheduler>().As<IReportStatusScheduler>().InstancePerLifetimeScope();
            builder.RegisterType<ConfigService>().As<IConfigService>().InstancePerLifetimeScope();
            builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();

            var mqConnectionFactory = new ConnectionFactory()
            {
                HostName = configuration["MessageQueueSettings:HostName"],
                UserName = configuration["MessageQueueSettings:UserName"],
                Password = configuration["MessageQueueSettings:Password"],
                VirtualHost = configuration["MessageQueueSettings:VirtualHost"]
            };
            builder.RegisterInstance(mqConnectionFactory).As<IConnectionFactory>().SingleInstance();
        }
    }
}
