using System.Net;
using System.Net.Http.Json;
using RoomsAPI.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace RoomsAPI.Tests.Controllers.ReservationsController;

// Existing reservation in all tests: 10:00-11:00 in the Small Room.
public class ReservationOverlapTests : ApiTestBase
{
	[Theory]
	[InlineData("10:30", "11:30")] // overlaps the end (example from the task)
	[InlineData("09:30", "10:30")] // overlaps the start
	[InlineData("10:15", "10:45")] // inside
	[InlineData("09:00", "12:00")] // around
	[InlineData("10:00", "11:00")] // same slot
	[InlineData("10:00", "10:30")] // same start
	[InlineData("10:30", "11:00")] // same end
	public async Task Overlapping_reservation_is_rejected(string start, string end)
	{
		await CreateReservation(SmallRoom, "10:00", "11:00");

		var response = await Book(SmallRoom, start, end);

		Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
	}

	[Theory]
	[InlineData("11:00", "12:00")] // starts when the existing one ends (example from the task)
	[InlineData("09:00", "10:00")] // ends when the existing one starts
	[InlineData("12:00", "13:00")] // later
	[InlineData("08:00", "09:00")] // earlier
	public async Task Adjacent_or_separate_reservation_is_accepted(string start, string end)
	{
		await CreateReservation(SmallRoom, "10:00", "11:00");

		var response = await Book(SmallRoom, start, end);

		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
	}

	[Fact]
	public async Task Same_time_in_another_room_is_accepted()
	{
		await CreateReservation(SmallRoom, "10:00", "11:00");

		var response = await Book(ConferenceRoom, "10:00", "11:00");

		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
	}

	[Fact]
	public async Task Reservation_overlapping_two_existing_ones_is_rejected()
	{
		await CreateReservation(SmallRoom, "10:00", "11:00");
		await CreateReservation(SmallRoom, "11:00", "12:00");

		var response = await Book(SmallRoom, "10:30", "11:30");

		Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
	}

	[Fact]
	public async Task Overlap_is_detected_when_times_are_sent_in_another_time_zone()
	{
		await CreateReservation(SmallRoom, "10:00", "11:00");
		var plusTwo = TimeSpan.FromHours(2);

		// 12:30+02:00 is 10:30 UTC
		var overlapping = await Book(SmallRoom, new DateTimeOffset(Day.AddHours(12.5), plusTwo), new DateTimeOffset(Day.AddHours(13), plusTwo));
		// 13:00+02:00 is 11:00 UTC, right after the existing one
		var adjacent = await Book(SmallRoom, new DateTimeOffset(Day.AddHours(13), plusTwo), new DateTimeOffset(Day.AddHours(14), plusTwo));

		Assert.Equal(HttpStatusCode.Conflict, overlapping.StatusCode);
		Assert.Equal(HttpStatusCode.Created, adjacent.StatusCode);
	}

	[Fact]
	public async Task Conflict_response_says_which_reservation_is_in_the_way()
	{
		var existing = await CreateReservation(SmallRoom, "10:00", "11:00");

		var response = await Book(SmallRoom, "10:30", "11:30");
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);

		Assert.Equal(409, problem!.Status);
		Assert.Contains("already booked", problem.Detail);
		Assert.Contains($"reservation {existing.Id}", problem.Detail);
	}

	[Fact]
	public async Task Rejected_reservation_is_not_saved()
	{
		await CreateReservation(SmallRoom, "10:00", "11:00");

		await Book(SmallRoom, "10:30", "11:30");

		var reservation = Assert.Single(await GetReservations(SmallRoom));
		Assert.Equal(At("10:00").UtcDateTime, reservation.Start);
	}

	[Fact]
	public async Task Parallel_requests_for_the_same_slot_book_it_only_once()
	{
		// 20 slots x 10 simultaneous requests each. Race conditions don't show up on every run,
		// so many attempts at once make a missing lock very likely to be caught.
		var slots = Enumerable.Range(0, 20).Select(i => At("00:00").AddHours(i)).ToList();
		var requests = slots.SelectMany(start => Enumerable.Range(0, 10).Select(_ => Book(SmallRoom, start, start.AddMinutes(30))));

		var responses = await Task.WhenAll(requests);

		Assert.Equal(slots.Count, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
		Assert.Equal(slots.Count * 9, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
		Assert.Equal(slots.Count, (await GetReservations(SmallRoom)).Count);
	}

	[Fact]
	public async Task Parallel_requests_for_different_overlapping_slots_book_only_one()
	{
		// all slots contain 10:20-10:25, so any two of them overlap and only one can win
		string[][] slots = [["09:00", "10:30"], ["10:00", "11:00"], ["10:15", "12:00"], ["10:20", "10:25"], ["09:45", "10:40"]];

		var responses = await Task.WhenAll(slots.Select(s => Book(SmallRoom, s[0], s[1])));

		Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
		Assert.Single(await GetReservations(SmallRoom));
	}
}
