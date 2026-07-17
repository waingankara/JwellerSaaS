using System.Net;
using JwellerSaaS.Shared.Responses;

namespace JwellerSaaS.Api.Middleware;

/// <summary>Converts unhandled exceptions to a standard error envelope.</summary>
public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled request exception.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var response = ApiResponse<object>.Fail(new ErrorResponse("internal_error", "An unexpected error occurred."));
            await context.Response.WriteAsJsonAsync(response, context.RequestAborted).ConfigureAwait(false);
        }
    }
}
