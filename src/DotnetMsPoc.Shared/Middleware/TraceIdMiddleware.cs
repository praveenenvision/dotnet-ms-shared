using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DotnetMsPoc.Shared.Middleware;

public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.Request.Headers["X-Trace-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(traceId))
        {
            traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        }

        context.Items["TraceId"] = traceId;
        context.Response.Headers["X-Trace-Id"] = traceId;

        using var activity = new ActivitySource("DotnetMsPoc").StartActivity("Request");
        activity?.SetTag("trace.id", traceId);

        await _next(context);
    }
}

public static class TraceIdMiddlewareExtensions
{
    public static IApplicationBuilder UseTraceIdMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TraceIdMiddleware>();
    }
}
