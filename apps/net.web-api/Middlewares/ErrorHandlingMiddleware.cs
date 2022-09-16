using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using adworks.data_services;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.media_web_api.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger logger)
        {
            this._next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode code;

            if (exception is NotFoundException)
            {
                code = HttpStatusCode.NotFound;
            }
            else if (exception is UnauthorizedException || exception is SecurityTokenExpiredException)
            {
                code = HttpStatusCode.Unauthorized;
            }
            else if (exception is NullReferenceException)
            {
                code = HttpStatusCode.InternalServerError;
            }
            else if (exception is InvalidRequstException)
            {
                code = HttpStatusCode.BadRequest;
            }
            else if (exception is ApplicationException)
            {
                code = HttpStatusCode.InternalServerError;
            }
            else if (exception is ValidationException)
            {
                code = HttpStatusCode.BadRequest;
            }
            else if (exception is InvalidLicenseException)
            {
                code = HttpStatusCode.BadRequest;
            }
            else
            {
                code = HttpStatusCode.InternalServerError; // 500 if unexpected
            }

            _logger.Error(exception,  "Unhandled error caught by error handling middleware");

            var result = JsonConvert.SerializeObject(new { error = exception.Message });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            await context.Response.WriteAsync(result);
        }
    }
}
