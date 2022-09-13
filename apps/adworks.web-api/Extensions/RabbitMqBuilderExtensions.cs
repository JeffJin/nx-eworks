using System.Diagnostics;
using System.Threading.Tasks;
using adworks.media_common;
using adworks.message_bus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace adworks.media_web_api.Services
{
    public static class RabbitMqBuilderExtensions
    {
        public static IRabbitListener Listener { get; set; }

        public static IApplicationBuilder UseRabbitListener(this IApplicationBuilder app)
        {
            Listener = app.ApplicationServices.GetService<IRabbitListener>();

            var life = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            
            life.ApplicationStarted.Register(OnStarted);
            
            //press Ctrl+C to reproduce if your app runs in Kestrel as a console app
            life.ApplicationStopping.Register(OnStopping);

            return app;
        }

        private static void OnStarted()
        {
            Listener.Register();
        }

        private static void OnStopping()
        {
            Listener.Deregister();
        }
    }
}