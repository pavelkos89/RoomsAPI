using System.ComponentModel.DataAnnotations;
using RoomsAPI.Dtos;

namespace RoomsAPI.Tests.Dtos;

public class CreateReservationRequestTests
{
	private static readonly DateTimeOffset Tomorrow = new(DateTime.UtcNow.Date.AddDays(1), TimeSpan.Zero);

	[Fact]
	public void Valid_request_has_no_errors()
	{
		var request = Request(Tomorrow.AddHours(10), Tomorrow.AddHours(11));

		Assert.Empty(Validate(request));
	}

	[Fact]
	public void End_before_start_is_invalid()
	{
		var request = Request(Tomorrow.AddHours(11), Tomorrow.AddHours(10));

		AssertSingleError(request, nameof(CreateReservationRequest.End));
	}

	[Fact]
	public void End_equal_to_start_is_invalid()
	{
		var request = Request(Tomorrow.AddHours(10), Tomorrow.AddHours(10));

		AssertSingleError(request, nameof(CreateReservationRequest.End));
	}

	[Fact]
	public void Start_in_the_past_is_invalid()
	{
		var start = DateTimeOffset.UtcNow.AddMinutes(-5);
		var request = Request(start, start.AddHours(1));

		AssertSingleError(request, nameof(CreateReservationRequest.Start));
	}

	[Fact]
	public void Reservation_longer_than_max_duration_is_invalid()
	{
		var request = Request(Tomorrow, Tomorrow + CreateReservationRequest.MaxDuration + TimeSpan.FromMinutes(1));

		AssertSingleError(request, nameof(CreateReservationRequest.End));
	}

	[Fact]
	public void Reservation_of_exactly_max_duration_is_valid()
	{
		var request = Request(Tomorrow, Tomorrow + CreateReservationRequest.MaxDuration);

		Assert.Empty(Validate(request));
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public void Empty_title_is_invalid(string title)
	{
		var request = Request(Tomorrow.AddHours(10), Tomorrow.AddHours(11), title);

		AssertSingleError(request, nameof(CreateReservationRequest.Title));
	}

	[Fact]
	public void Title_longer_than_200_characters_is_invalid()
	{
		var request = Request(Tomorrow.AddHours(10), Tomorrow.AddHours(11), new string('a', 201));

		AssertSingleError(request, nameof(CreateReservationRequest.Title));
	}

	[Fact]
	public void Missing_start_and_end_are_invalid()
	{
		var request = new CreateReservationRequest { Title = "Meeting" };

		var errors = Validate(request).SelectMany(e => e.MemberNames).ToList();

		Assert.Contains(nameof(CreateReservationRequest.Start), errors);
		Assert.Contains(nameof(CreateReservationRequest.End), errors);
	}

	private static CreateReservationRequest Request(DateTimeOffset start, DateTimeOffset end, string title = "Meeting") =>
		new() { Start = start, End = end, Title = title };

	private static List<ValidationResult> Validate(CreateReservationRequest request)
	{
		var results = new List<ValidationResult>();
		Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
		return results;
	}

	private static void AssertSingleError(CreateReservationRequest request, string member)
	{
		var error = Assert.Single(Validate(request));
		Assert.Equal([member], error.MemberNames);
	}
}
