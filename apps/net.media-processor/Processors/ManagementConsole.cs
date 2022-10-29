using System;
using adworks.message_bus;
using Eworks.AdworksMediaProcessor.Processors;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MediaProcessor
{
  /// <summary>
  /// For measuring the performance of each task
  ///
  /// </summary>
  public class ManagementConsole : BaseProcessor, IManagementConsole
  {
    public ManagementConsole(IConfiguration configuration, ILogger logger, IMessageClient messageClient) : base(
      configuration, logger, messageClient)
    {
    }

    public void Dispose()
    {
    }

    public void Run()
    {
      //throw new NotImplementedException();
    }
  }
}
