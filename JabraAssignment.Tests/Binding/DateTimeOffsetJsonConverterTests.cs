using JabraAssignment.Binding;

namespace JabraAssignment.Tests.Binding;

public class DateTimeOffsetJsonConverterTests
{
	[Theory]
	[InlineData("2030-10-01T10:00:00Z", 0)]
	[InlineData("2030-10-01T10:00:00z", 0)]
	[InlineData("2030-10-01T10:00:00+00:00", 0)]
	[InlineData("2030-10-01T12:00:00+02:00", 2)]
	[InlineData("2030-10-01T05:00:00-05:00", -5)]
	[InlineData("2030-10-01T10:00:00.123Z", 0)]
	public void Accepts_values_with_offset(string text, int offsetHours)
	{
		Assert.True(DateTimeOffsetJsonConverter.TryParse(text, out var value));
		Assert.Equal(TimeSpan.FromHours(offsetHours), value.Offset);
	}

	[Theory]
	[InlineData("2030-10-01T10:00:00")] // no offset - would be read as server local time
	[InlineData("2030-10-01")]
	[InlineData("tomorrow Z")]
	[InlineData("")]
	[InlineData(null)]
	public void Rejects_values_without_offset_or_invalid(string? text)
	{
		Assert.False(DateTimeOffsetJsonConverter.TryParse(text, out _));
	}

	[Fact]
	public void Offset_is_kept_so_utc_conversion_is_correct()
	{
		DateTimeOffsetJsonConverter.TryParse("2030-10-01T12:30:00+02:00", out var value);

		Assert.Equal(new DateTime(2030, 10, 1, 10, 30, 0, DateTimeKind.Utc), value.UtcDateTime);
	}
}
