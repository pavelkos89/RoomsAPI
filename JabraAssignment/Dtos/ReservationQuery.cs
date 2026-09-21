using System.ComponentModel.DataAnnotations;
using JabraAssignment.Configuration;
using Microsoft.Extensions.Options;

namespace JabraAssignment.Dtos;

public class ReservationQuery : IValidatableObject
{
	public DateTimeOffset? From { get; set; }

	public DateTimeOffset? To { get; set; }

	[Range(1, int.MaxValue)]
	public int Page { get; set; } = 1;

	// when not set, DefaultPageSize from configuration is used
	public int? PageSize { get; set; }

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (From.HasValue && To.HasValue && From >= To)
		{
			yield return new ValidationResult("To must be after From.", [nameof(To)]);
		}

		var paging = validationContext.GetRequiredService<IOptions<PagingOptions>>().Value;

		if (PageSize < 1 || PageSize > paging.MaxPageSize)
		{
			yield return new ValidationResult($"PageSize must be between 1 and {paging.MaxPageSize}.", [nameof(PageSize)]);
		}

		// (Page - 1) * PageSize must fit into int
		if (Page > int.MaxValue / paging.MaxPageSize)
		{
			yield return new ValidationResult("Page is too large.", [nameof(Page)]);
		}
	}
}
