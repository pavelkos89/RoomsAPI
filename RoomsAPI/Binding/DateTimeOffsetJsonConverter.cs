using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RoomsAPI.Binding;

// Without an explicit offset .NET would treat "2030-10-01T10:00:00" as the server's local time,
// so the same request would book different times on different servers. We reject such values instead.
public partial class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
	public const string ErrorMessage = "Expected a date and time with a time zone offset, e.g. 2030-10-01T10:00:00Z or 2030-10-01T12:00:00+02:00.";

	public static bool TryParse(string? text, out DateTimeOffset value)
	{
		value = default;

		return text != null
			&& EndsWithOffset().IsMatch(text)
			&& DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
	}

	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String || !TryParse(reader.GetString(), out var value))
		{
			throw new JsonException(ErrorMessage);
		}

		return value;
	}

	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
		writer.WriteStringValue(value);

	[GeneratedRegex(@"(Z|[+-]\d{2}:\d{2})$", RegexOptions.IgnoreCase)]
	private static partial Regex EndsWithOffset();
}
