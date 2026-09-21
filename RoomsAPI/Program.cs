using RoomsAPI.Data;
using RoomsAPI.Extensions;
using RoomsAPI.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.MapControllers();

app.Run();
