using System;
using System.Linq;
using System.Text;
using adworks.message_bus;
using Eworks.AdworksMediaProcessor.Processors;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MediaProcessor
{
  /// <summary>
  /// Process manager will coordinate between different tasks
  /// 1. Check message queue to find if there is any new file to process
  /// 2. Check for failed tasks and
  /// </summary>
  public class ProcessManager : BaseProcessor, IProcessManager
  {
    public ProcessManager(IConfiguration configuration, ILogger logger, IMessageClient messageClient) : base(
      configuration, logger, messageClient)
    {
    }

    public void Dispose()
    {
    }

    public void Run()
    {
      throw new NotImplementedException();
    }
  }
}
