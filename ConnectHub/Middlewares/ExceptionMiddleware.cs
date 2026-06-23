using ConnectHub.Application.DTO_s.Common;
using System.Net;
using System.Text.Json;

namespace ConnectHub.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next,ILogger<ExceptionMiddleware> logger,IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.ContentType= "application/json";
                context.Response.StatusCode = 500;

                var response = _env.IsDevelopment() ?
                    new ApiExceptionResponse(500, ex.Message, ex.InnerException?.Message)
                    : new ApiExceptionResponse(500);
                var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var returned = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(returned);
            }

        }
       
    }
}
