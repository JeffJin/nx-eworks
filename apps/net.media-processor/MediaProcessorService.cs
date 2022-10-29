using Microsoft.Extensions.Hosting;
using Autofac;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_processor
{
  public class MediaProcessorService : IHostedService
  {
    private IEnumerable<IProcessor> _processors;
    private ILogger _logger;

    public MediaProcessorService()
    {
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      //configure autofac DI
      var configResult = ContainerConfig.Configure();

      using (var scope = configResult.Container.BeginLifetimeScope())
      {
        //start all processors
        _processors = scope.Resolve<IEnumerable<IProcessor>>();
        _logger = scope.Resolve<ILogger>();
        _logger.Information("Media Processor Service is starting.");

        var tasks = _processors.ToArray().Select(processor => Task.Run(() =>
        {
          processor.Run();
          // Keep the processor running.
          Thread.Sleep(Timeout.Infinite);
        })).ToArray();
        _logger.Information("Media Processor Service is working.");

        _logger.Information("Ctrl-c to quit media processor");
        return Task.WhenAll(tasks);
      }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      _logger?.Information("Media Processor Service is stopping.");

      var tasks = _processors.ToArray().Select(processor => Task.Run(() =>
      {
        processor.Stop();
      })).ToArray();

      return Task.WhenAll(tasks);
    }
  }
}

