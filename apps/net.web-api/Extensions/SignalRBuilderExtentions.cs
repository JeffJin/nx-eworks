using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;

namespace adworks.media_web_api.Services
{
    public static class SignalRBuilderExtensions
    {

        public static IApplicationBuilder UseSignalRSecurity(this IApplicationBuilder app)
        {
            app
                .Use(async (context, next) =>
                {
                    if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
                    {
                        try
                        {
                            if (context.Request.QueryString.HasValue)
                            {
                                var token = context.Request.QueryString.Value
                                    .Split('&')
                                    .SingleOrDefault(x =>
                                    {
                                        var i = x.IndexOf("t=", StringComparison.CurrentCultureIgnoreCase);
                                        return i > -1 && i < 2;
                                    })?
                                    .Split('=')
                                    .LastOrDefault();

                                if (!string.IsNullOrWhiteSpace(token) &&
                                    string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
                                {
                                    context.Request.Headers.Add("Authorization", new[] {$"Bearer {token}"});
                                }
                            }
                        }
                        catch
                        {
                            // if multiple headers it may throw an error.  Ignore both.
                        }
                    }

                    await next();
                });            

            return app;
        }

       
    }
}