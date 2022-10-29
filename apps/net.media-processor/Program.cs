using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace adworks.media_processor
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      // await Host.CreateDefaultBuilder(args)
      //   .ConfigureServices(services =>
      //   {
      //     services.AddHostedService<MediaProcessorService>();
      //   })
      //   .Build()
      //   .RunAsync();
      var hostBuilder = new HostBuilder()
        // Add configuration, logging, ...
        .ConfigureServices((hostContext, services) =>
        {
          services.AddHostedService<MediaProcessorService>();
        });

      await hostBuilder.RunConsoleAsync();
    }
  }
}
