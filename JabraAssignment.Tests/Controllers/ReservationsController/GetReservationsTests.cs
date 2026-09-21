using System.Net;
using JabraAssignment.Tests.Infrastructure;

namespace JabraAssignment.Tests.Controllers.ReservationsController;

public class GetReservationsTests : ApiTestBase
{
	[Fact]
	public async Task Returns_reservations_of_the_room_ordered_by_start()
	{
		await CreateReservation(SmallRoom, "12:00", "13:00");
		await CreateReservation(SmallRoom, "10:00", "11:00");
		await CreateReservation(ConferenceRoom, "09:00", "10:00");

		var reservations = await GetReservations(SmallRoom);

		Assert.Equal([At("10:00").UtcDateTime, At("12:00").UtcDateTime], reservations.Select(r => r.Start));
	}

	[Theory]
	[InlineData("10:30", "11:30", "10:00,11:00")] // partly overlapping reservations are included
	[InlineData("11:00", "13:00", "11:00")]       // 10-11 ends at 'from' and 13-14 starts at 'to', both excluded
	[InlineData("10:15", "10:45", "10:00")]       // range inside a reservation
	[InlineData("08:00", "09:00", "")]            // nothing in range
	public async Task Returns_reservations_overlapping_the_range(string from, string to, string expectedStarts)
	{
		await CreateReservation(SmallRoom, "10:00", "11:00");
		await CreateReservation(SmallRoom, "11:00", "12:00");
		await CreateReservation(SmallRoom, "13:00", "14:00");

		var reservations = await GetReservations(SmallRoom, $"?from={Format(At(from))}&to={Format(At(to))}");

		Assert.Equal(expectedStarts, string.Join(",", reservations.Select(r => r.Start.ToString("HH:mm"))));
	}

	[Fact]
	public async Task Range_can_be_open_on_one_side()
	{
		await CreateReservation(SmallRoom, "10:00", "11:00");
		await CreateReservation(SmallRoom, "12:00", "13:00");

		var fromOnly = await GetReservations(SmallRoom, $"?from={Format(At("11:30"))}");
		var toOnly = await GetReservations(SmallRoom, $"?to={Format(At("11:30"))}");

		Assert.Equal(At("12:00").UtcDateTime, Assert.Single(fromOnly).Start);
		Assert.Equal(At("10:00").UtcDateTime, Assert.Single(toOnly).Start);
	}

	[Fact]
	public async Task Results_are_paged()
	{
		await CreateReservation(SmallRoom, "08:00", "08:30");
		await CreateReservation(SmallRoom, "09:00", "09:30");
		await CreateReservation(SmallRoom, "10:00", "10:30");
		await CreateReservation(SmallRoom, "11:00", "11:30");
		await CreateReservation(SmallRoom, "12:00", "12:30");

		var page2 = await GetReservations(SmallRoom, "?pageSize=2&page=2");
		var page3 = await GetReservations(SmallRoom, "?pageSize=2&page=3");
		var page4 = await GetReservations(SmallRoom, "?pageSize=2&page=4");

		Assert.Equal([At("10:00").UtcDateTime, At("11:00").UtcDateTime], page2.Select(r => r.Start));
		Assert.Single(page3);
		Assert.Empty(page4);
	}

	[Theory]
	[InlineData("?pageSize=0")]
	[InlineData("?pageSize=101")]
	[InlineData("?page=0")]
	[InlineData("?page=2147483647")]
	[InlineData("?from=not-a-date")]
	[InlineData("?from=2030-10-01T10:00:00")] // no offset
	[InlineData("?to=2030-10-01T10:00:00")]
	public async Task Invalid_query_returns_bad_request(string query)
	{
		var response = await Client.GetAsync($"/api/rooms/{SmallRoom}/reservations{query}", Ct);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task Range_accepts_offsets_other_than_utc()
	{
		await CreateReservation(SmallRoom, "10:00", "11:00");
		await CreateReservation(SmallRoom, "12:00", "13:00");

		// 13:30+02:00 is 11:30 UTC; '+' has to be encoded as %2B in a URL
		var from = new DateTimeOffset(Day.AddHours(13.5), TimeSpan.FromHours(2)).ToString("yyyy-MM-ddTHH:mm:sszzz").Replace("+", "%2B");
		var reservations = await GetReservations(SmallRoom, $"?from={from}");

		Assert.Equal(At("12:00").UtcDateTime, Assert.Single(reservations).Start);
	}

	[Fact]
	public async Task Range_where_to_is_not_after_from_returns_bad_request()
	{
		var response = await Client.GetAsync(
			$"/api/rooms/{SmallRoom}/reservations?from={Format(At("12:00"))}&to={Format(At("10:00"))}", Ct);

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task Unknown_room_returns_not_found()
	{
		var response = await Client.GetAsync("/api/rooms/999/reservations", Ct);

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	// '+' in a query string means a space, so dates are sent in UTC with 'Z'
	private static string Format(DateTimeOffset value) => value.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
}
