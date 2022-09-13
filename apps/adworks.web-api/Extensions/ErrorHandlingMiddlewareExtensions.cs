using adworks.media_web_api.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace adworks.media_web_api.Extensions
{
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}