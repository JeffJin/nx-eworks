using System.IO;
using System.Security.Authentication;
using Autofac;
using adworks.data_services;
using adworks.ffmpeg_media_services;
using adworks.media_common;
using adworks.media_common.Configuration;
using adworks.media_common.Services;
using adworks.media_processor.Processors;
using adworks.message_bus;
using adworks.message_common;
using adworks.networking;
using adworks.scheduler;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using ILogger = Serilog.ILogger;

namespace adworks.media_processor
{
    public class ProcessorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var configuration = AppConfig.GetConfig();

            builder.Register<ILogger>((c, p) =>
            {
                var loggerConfig = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration);

                if (!string.IsNullOrWhiteSpace(configuration["LogFile"]))
                {
                    loggerConfig
                        .Enrich.WithExceptionDetails()
                        .WriteTo.RollingFile(
                            Path.Combine(configuration["BaseAssetFolder"], configuration["LogFile"]),
                            outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}");
                }

                var logger = loggerConfig
                    .WriteTo.Console(
                        outputTemplate: "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}")
                    .CreateLogger();

                Log.Logger = logger;
                return logger;

            }).SingleInstance();

            builder.RegisterType<VideoProcessor>().As<IProcessor>();
            builder.RegisterType<AudioProcessor>().As<IProcessor>();
            builder.RegisterType<ImageProcessor>().As<IProcessor>();
            builder.RegisterType<EventLoggingProcessor>().As<IProcessor>();

            builder.RegisterType<MessageClient>().As<IMessageClient>().SingleInstance();
            builder.RegisterType<TextService>().As<ITextService>().InstancePerLifetimeScope();
            builder.RegisterType<CommonDataService>().As<ICommonDataService>().InstancePerLifetimeScope();
            builder.RegisterType<VideoService>().As<IVideoService>().InstancePerLifetimeScope();
            builder.RegisterType<AudioService>().As<IAudioService>().InstancePerLifetimeScope();
            builder.RegisterType<ImageService>().As<IImageService>().InstancePerLifetimeScope();
            builder.RegisterType<OrganizationService>().As<IOrganizationService>().InstancePerLifetimeScope();
            builder.RegisterType<DeviceService>().As<IDeviceService>().InstancePerLifetimeScope();
            builder.RegisterType<DeviceGroupService>().As<IDeviceGroupService>().InstancePerLifetimeScope();
            builder.RegisterType<LocationService>().As<ILocationService>().InstancePerLifetimeScope();
            builder.RegisterType<PlaylistService>().As<IPlaylistService>().InstancePerLifetimeScope();
            builder.RegisterType<ReportService>().As<IReportService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();

            builder.RegisterType<FFMpegMediaService>().As<IFFMpegService>().InstancePerLifetimeScope();
            builder.RegisterType<FileService>().As<IFileService>().InstancePerLifetimeScope();
            builder.RegisterType<FtpService>().As<IFtpService>().InstancePerLifetimeScope();
            builder.RegisterType<SocketService>().As<ISocketService>().InstancePerLifetimeScope();
            builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<SmsService>().As<ISmsService>().InstancePerLifetimeScope();
            builder.RegisterType<ScheduleService>().As<IScheduleService>().InstancePerLifetimeScope();
            builder.RegisterType<DataContextFactory>().As<IDataContextFactory>().InstancePerLifetimeScope();

//            builder.RegisterType<AdSuggeestionService>().As<IAdSuggeestionService>().InstancePerLifetimeScope();
//            builder.RegisterType<AdSearchEngine>().As<IAdSearchEngine>().InstancePerLifetimeScope();
            builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();

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
