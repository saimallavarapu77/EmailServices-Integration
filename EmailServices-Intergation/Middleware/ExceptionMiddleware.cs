using EmailServices.Application.Common;
using Serilog;
using System.Net;
using System.Text.Json;

namespace EmailServices.API.Middleware;

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
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            Log.Error(ex, "Unhandled exception occurred");

            var response = ApiResponse<object>.FailureResponse(
                "An unexpected error occurred.");

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
