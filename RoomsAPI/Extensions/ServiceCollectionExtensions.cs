using RoomsAPI.Binding;
using RoomsAPI.Configuration;
using RoomsAPI.Data;
using RoomsAPI.Middleware;
using RoomsAPI.Repositories;
using RoomsAPI.Repositories.Interfaces;
using RoomsAPI.Services;
using RoomsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RoomsAPI.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite(configuration.GetConnectionString("RoomBooking")));

		services.Configure<PagingOptions>(configuration.GetSection(PagingOptions.SectionName));

		services.AddScoped<IRoomRepository, RoomRepository>();
		services.AddScoped<IRoomService, RoomService>();
		services.AddScoped<IReservationRepository, ReservationRepository>();
		services.AddScoped<IReservationService, ReservationService>();

		services.AddProblemDetails();
		services.AddExceptionHandler<GlobalExceptionHandler>();

		services
			.AddControllers(options =>
			{
				options.ModelBinderProviders.Insert(0, new DateTimeOffsetModelBinderProvider());
				// DTOs use explicit [Required]; the implicit one only adds a confusing "request field is required" error
				options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
			})
			.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new DateTimeOffsetJsonConverter()))
			.ConfigureApiBehaviorOptions(LogValidationErrors);
		services.AddOpenApi();

		return services;
	}

	// [ApiController] returns 400 automatically when validation fails; log the reason before that happens.
	// Only error messages are logged, not the submitted values.
	private static void LogValidationErrors(ApiBehaviorOptions options)
	{
		var createResponse = options.InvalidModelStateResponseFactory;

		options.InvalidModelStateResponseFactory = context =>
		{
			var errors = context.ModelState
				.Where(entry => entry.Value?.Errors.Count > 0)
				.Select(entry => $"{(entry.Key == "" ? "body" : entry.Key)}: {string.Join(" ", entry.Value!.Errors.Select(e => e.ErrorMessage))}");

			context.HttpContext.RequestServices
				.GetRequiredService<ILoggerFactory>()
				.CreateLogger("RoomsAPI.Validation")
				.LogInformation("Request validation failed: {Errors}", string.Join(" | ", errors));

			return createResponse(context);
		};
	}
}
