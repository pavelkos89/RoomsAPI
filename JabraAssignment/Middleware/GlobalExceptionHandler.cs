using JabraAssignment.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JabraAssignment.Middleware;

public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
	: IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
	{
		var (status, title) = exception switch
		{
			NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
			ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
			_ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
		};

		if (status == StatusCodes.Status500InternalServerError)
		{
			logger.LogError(exception, "Unhandled exception");
		}

		httpContext.Response.StatusCode = status;

		return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = httpContext,
			ProblemDetails = new ProblemDetails
			{
				Status = status,
				Title = title,
				// don't expose internal error messages to the client
				Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message
			}
		});
	}
}
