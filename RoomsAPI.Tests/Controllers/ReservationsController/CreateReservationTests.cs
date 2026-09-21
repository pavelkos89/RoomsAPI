using System.Net;
using System.Net.Http.Json;
using RoomsAPI.Models;
using RoomsAPI.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace RoomsAPI.Tests.Controllers.ReservationsController;

public class CreateReservationTests : ApiTestBase
{
	[Fact]
	public async Task Valid_reservation_is_created()
	{
		var before = DateTime.UtcNow;

		var response = await Book(SmallRoom, At("10:00"), At("11:00"), "Team sync");
		var reservation = await response.Content.ReadFromJsonAsync<Reservation>(Ct);

		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		Assert.True(reservation!.Id > 0);
		Assert.Equal(SmallRoom, reservation.RoomId);
		Assert.Equal(At("10:00").UtcDateTime, reservation.Start);
		Assert.Equal(At("11:00").UtcDateTime, reservation.End);
		Assert.Equal("Team sync", reservation.Title);
		Assert.InRange(reservation.CreatedAt, before, DateTime.UtcNow);
	}

	[Fact]
	public async Task Unknown_room_returns_not_found()
	{
		var response = await Book(999, "10:00", "11:00");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task Invalid_request_returns_bad_request_and_is_not_saved()
	{
		var response = await Book(SmallRoom, "11:00", "10:00");

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.Empty(await GetReservations(SmallRoom));
	}

	[Fact]
	public async Task Missing_fields_return_bad_request()
	{
		var response = await Client.PostAsJsonAsync($"/api/rooms/{SmallRoom}/reservations", new { title = "No times" }, Ct);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task Times_without_offset_are_rejected_and_not_saved()
	{
		// without the check, this would be booked in the server's local time zone
		var day = Day.ToString("yyyy-MM-dd");
		var json = $$"""{ "start": "{{day}}T10:00:00", "end": "{{day}}T11:00:00", "title": "Meeting" }""";

		var response = await Client.PostAsync($"/api/rooms/{SmallRoom}/reservations",
			new StringContent(json, System.Text.Encoding.UTF8, "application/json"), Ct);
		var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		Assert.Contains("time zone offset", Assert.Single(problem!.Errors["$.start"]));
		Assert.Empty(await GetReservations(SmallRoom));
	}

	[Fact]
	public async Task Empty_body_returns_bad_request()
	{
		var content = new StringContent("", System.Text.Encoding.UTF8, "application/json");

		var response = await Client.PostAsync($"/api/rooms/{SmallRoom}/reservations", content, Ct);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task Malformed_json_returns_bad_request()
	{
		var content = new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json");

		var response = await Client.PostAsync($"/api/rooms/{SmallRoom}/reservations", content, Ct);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}
}
