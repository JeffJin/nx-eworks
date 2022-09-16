using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_processor
{
    class Program
    {

        static void Main(string[] args)
        {
            //configure autofac DI
            var configResult = ContainerConfig.Configure();

            using (var scope = configResult.Container.BeginLifetimeScope())
            {
                //start all processors
                var processors = scope.Resolve<IEnumerable<IProcessor>>();

                var tasks = processors.ToArray().Select(processor => Task.Run(() =>
                {
                    processor.Run();
                    // Keep the processor running.
                    Thread.Sleep(Timeout.Infinite);
                })).ToArray();

                var logger = scope.Resolve<ILogger>();

                logger.Information("Ctrl-c to quit media processor");
                Task.WaitAll(tasks);
            }
        }

    }
}
