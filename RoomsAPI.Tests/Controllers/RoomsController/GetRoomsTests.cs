using System.Net.Http.Json;
using RoomsAPI.Models;
using RoomsAPI.Tests.Infrastructure;

namespace RoomsAPI.Tests.Controllers.RoomsController;

public class GetRoomsTests : ApiTestBase
{
	[Fact]
	public async Task Returns_sample_rooms()
	{
		var rooms = await Client.GetFromJsonAsync<List<Room>>("/api/rooms", Ct);

		Assert.Equal(3, rooms!.Count);
		Assert.Contains(rooms, r => r is { Name: "Small Room", Capacity: 4 });
		Assert.Contains(rooms, r => r is { Name: "Conference Room", Capacity: 10 });
		Assert.Contains(rooms, r => r is { Name: "Auditorium", Capacity: 50 });
	}
}
