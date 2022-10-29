using adworks.message_bus;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Eworks.AdworksMediaProcessor.Processors
{
  public abstract class BaseProcessor
  {
    protected ISubscription _subscription;
    protected readonly ILogger _logger;
    protected readonly IConfiguration _configuration;
    protected readonly IMessageClient _messageClient;

    protected readonly IList<Task> _tasks = new List<Task>();

    public BaseProcessor( IConfiguration configuration, ILogger logger, IMessageClient messageClient)
    {
      _logger = logger;
      _configuration = configuration;
      _messageClient = messageClient;
    }

    public void Stop()
    {
      _subscription.Unsubscribe();
      //stop all running tasks
      foreach (var task in _tasks)
      {
        try
        {
          task.Dispose();
        }
        catch (Exception e)
        {
          _logger.Error("Unable to dispose the task", e);
        }
      }
    }
  }

}

