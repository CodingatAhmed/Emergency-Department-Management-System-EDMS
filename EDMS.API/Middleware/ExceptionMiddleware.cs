using System.Net;
using System.Text.Json;

namespace EDMS.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException => (int)HttpStatusCode.UnprocessableEntity,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var payload = new
            {
                success = false,
                error = new
                {
                    code = context.Response.StatusCode switch
                    {
                        400 => "VALIDATION_ERROR",
                        404 => "NOT_FOUND",
                        422 => "INVALID_OPERATION",
                        _ => "INTERNAL_ERROR"
                    },
                    message = ex.Message
                },
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
