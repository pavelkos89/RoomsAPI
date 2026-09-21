using System.ComponentModel.DataAnnotations;

namespace RoomsAPI.Dtos;

public class CreateReservationRequest : IValidatableObject
{
	public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(24);

	[Required]
	public DateTimeOffset? Start { get; set; }

	[Required]
	public DateTimeOffset? End { get; set; }

	[Required]
	[StringLength(200)]
	public string Title { get; set; } = string.Empty;

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (Start < DateTimeOffset.UtcNow)
		{
			yield return new ValidationResult("Start must not be in the past.", [nameof(Start)]);
		}

		if (Start >= End)
		{
			yield return new ValidationResult("End must be after Start.", [nameof(End)]);
		}
		else if (End - Start > MaxDuration)
		{
			yield return new ValidationResult($"Reservation can't be longer than {MaxDuration.TotalHours} hours.", [nameof(End)]);
		}
	}
}
