using System.IO;
using System.Security.Authentication;
using Autofac;
using adworks.data_services;
using adworks.media_common;
using adworks.media_common.Configuration;
using adworks.message_bus;
using adworks.message_common;
using adworks.networking;
using adworks.scheduler;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
using ILogger = Serilog.ILogger;
using Serilog.Events;
using Serilog.Exceptions;

namespace adworks.media_web_api
{
    public class WebApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var configuration = AppConfig.GetConfig();

            builder.Register<ILogger>((c, p) =>
            {
                var logger =  new LoggerConfiguration()
                    .Enrich.WithExceptionDetails()
                    .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}",
                        restrictedToMinimumLevel:LogEventLevel.Debug)
                    .WriteTo.RollingFile(Path.Combine(configuration["BaseAssetFolder"], configuration["LogFile"]),
                        LogEventLevel.Information, outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}")
                    .CreateLogger();

                Log.Logger = logger;
                return logger;

            }).SingleInstance();

            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.RegisterType<RabbitUserSubscriptionService>().As<IRabbitListener>().SingleInstance();
            builder.RegisterType<CommonDbContext>();
            builder.RegisterType<MessageClient>().As<IMessageClient>().SingleInstance();
            builder.RegisterType<UserQueueMessageHandler>().As<IUserQueueMessageHandler>().InstancePerLifetimeScope();

            builder.RegisterType<CommonDataService>().As<ICommonDataService>().InstancePerLifetimeScope();
            builder.RegisterType<VideoService>().As<IVideoService>().InstancePerLifetimeScope();
            builder.RegisterType<AudioService>().As<IAudioService>().InstancePerLifetimeScope();
            builder.RegisterType<ImageService>().As<IImageService>().InstancePerLifetimeScope();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
            builder.RegisterType<OrganizationService>().As<IOrganizationService>().InstancePerLifetimeScope();
            builder.RegisterType<LicenseService>().As<ILicenseService>().InstancePerLifetimeScope();
            builder.RegisterType<DeviceService>().As<IDeviceService>().InstancePerLifetimeScope();
            builder.RegisterType<DeviceGroupService>().As<IDeviceGroupService>().InstancePerLifetimeScope();
            builder.RegisterType<LocationService>().As<ILocationService>().InstancePerLifetimeScope();
            builder.RegisterType<PlaylistService>().As<IPlaylistService>().InstancePerLifetimeScope();
            builder.RegisterType<ReportService>().As<IReportService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();

            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();
            builder.RegisterType<FtpService>().As<IFtpService>().InstancePerLifetimeScope();
            builder.RegisterType<SocketService>().As<ISocketService>().InstancePerLifetimeScope();
            builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<SmsService>().As<ISmsService>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleService>().As<IScheduleService>().InstancePerLifetimeScope();

            builder.RegisterType<SendNotificationsJob>().As<INotificationJob>().InstancePerLifetimeScope();
            builder.RegisterType<DataContextFactory>().As<IDataContextFactory>().InstancePerLifetimeScope();
            builder.RegisterType<EventHub>().As<IEventHubConnections>().InstancePerLifetimeScope();
            //rabbit message queue connection settings
            var messageQueueSettings = configuration.GetSection("MessageQueueSettings").Get<MessageQueueSettings>();

            var mqConnectionFactory = new ConnectionFactory()
            {
                HostName = messageQueueSettings.HostName,
                UserName = messageQueueSettings.UserName,
                Password = messageQueueSettings.Password,
                VirtualHost = messageQueueSettings.VirtualHost,
                Port = messageQueueSettings.Port,
            };

            if (messageQueueSettings.Ssl)
            {
                mqConnectionFactory.Ssl = new SslOption()
                {
                    Enabled = true,
                    ServerName = messageQueueSettings.HostName,
                    Version = SslProtocols.Tls12,
                };
            }
            builder.RegisterInstance(mqConnectionFactory).As<IConnectionFactory>().SingleInstance();
        }
    }
}
