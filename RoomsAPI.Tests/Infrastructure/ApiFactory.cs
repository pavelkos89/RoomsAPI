using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace RoomsAPI.Tests.Infrastructure;

// Runs the real API in memory with its own SQLite file, so every test starts with a clean database.
// A file (not an in-memory database) is used because parallel requests need separate connections.
public class ApiFactory : WebApplicationFactory<Program>
{
	private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"roombooking-test-{Guid.NewGuid()}.db");

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseSetting("ConnectionStrings:RoomBooking", $"Data Source={_databasePath}");
	}

	public override async ValueTask DisposeAsync()
	{
		await base.DisposeAsync();

		SqliteConnection.ClearAllPools();
		File.Delete(_databasePath);
	}
}
