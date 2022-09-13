using System;
using System.Collections.Generic;
using System.IO;
using adworks.message_bus;
using adworks.pi_player.Logging;
using adworks.pi_player.Processors;
using Autofac;
using Quartz;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.pi_player
{
    class Program
    {

        static void Main(string[] args)
        {
            //configure autofac DI
            var configResult = ContainerConfig.Configure();
            Log.Logger.Information("Start listening to playlist messages...");

            var subscriptions = new List<ISubscription>();

            using (var scope = configResult.Container.BeginLifetimeScope())
            {
                var dataService = scope.Resolve<IDataService>();
                dataService.Init().GetAwaiter().GetResult();

                //start media processor
                var playlistProcessor = scope.Resolve<IPlaylistProcessor>();
                ISubscription playlistSubscription = playlistProcessor.Run().GetAwaiter().GetResult();
                subscriptions.Add(playlistSubscription);

//                var auditProcessor = scope.Resolve<IAuditProcessor>();
//                ISubscription auditSubscription = auditProcessor.Run().GetAwaiter().GetResult();
//                subscriptions.Add(auditSubscription);

                //restart scheduled jobs
                var scheduler = scope.Resolve<IScheduler>();
                scheduler.Start().GetAwaiter().GetResult();

                var logger = scope.Resolve<ILogger>();
                logger.Information("Press x to exit");
                var info = Console.ReadKey();
                while (info.KeyChar != 'x')
                {
                    info = Console.ReadKey();
                }

                logger.Information("Quartz scheduler is shutting down!");

                //stop the quartz scheduler
                if (!scheduler.IsShutdown)
                {
                    scheduler.Shutdown();
                }

                foreach (var sub in subscriptions)
                {
                    sub.Dispose();
                }

                logger.Information("Goodbye!");
            }


        }

    }



}
