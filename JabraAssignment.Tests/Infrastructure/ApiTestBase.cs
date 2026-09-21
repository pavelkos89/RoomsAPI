using System.Net;
using System.Net.Http.Json;
using JabraAssignment.Models;

namespace JabraAssignment.Tests.Infrastructure;

public abstract class ApiTestBase : IAsyncDisposable
{
	protected const int SmallRoom = 1;
	protected const int ConferenceRoom = 2;

	// a week ahead, so the "no reservations in the past" rule doesn't affect the tests.
	// Kind is Unspecified so it can be combined with any offset in new DateTimeOffset(...)
	protected static readonly DateTime Day = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(7), DateTimeKind.Unspecified);

	private readonly ApiFactory _factory = new();

	protected ApiTestBase()
	{
		Client = _factory.CreateClient();
	}

	protected HttpClient Client { get; }

	protected static CancellationToken Ct => TestContext.Current.CancellationToken;

	protected static DateTimeOffset At(string time) => new(Day + TimeSpan.Parse(time), TimeSpan.Zero);

	protected Task<HttpResponseMessage> Book(int roomId, string start, string end) =>
		Book(roomId, At(start), At(end));

	protected Task<HttpResponseMessage> Book(int roomId, DateTimeOffset start, DateTimeOffset end, string title = "Meeting") =>
		Client.PostAsJsonAsync($"/api/rooms/{roomId}/reservations", new { start, end, title }, Ct);

	// for test setup: books a slot and fails the test if it wasn't created
	protected async Task<Reservation> CreateReservation(int roomId, string start, string end)
	{
		var response = await Book(roomId, start, end);
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);

		return (await response.Content.ReadFromJsonAsync<Reservation>(Ct))!;
	}

	protected async Task<List<Reservation>> GetReservations(int roomId, string query = "")
	{
		var response = await Client.GetAsync($"/api/rooms/{roomId}/reservations{query}", Ct);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		return (await response.Content.ReadFromJsonAsync<List<Reservation>>(Ct))!;
	}

	public async ValueTask DisposeAsync()
	{
		Client.Dispose();
		await _factory.DisposeAsync();
		GC.SuppressFinalize(this);
	}
}
