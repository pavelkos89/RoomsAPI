using System.Diagnostics;

namespace JabraAssignment.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		var startTime = Stopwatch.GetTimestamp();

		try
		{
			await next(context);
		}
		finally
		{
			logger.LogInformation("{Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs:0} ms",
				context.Request.Method,
				context.Request.Path,
				context.Request.QueryString,
				context.Response.StatusCode,
				Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
		}
	}
}
