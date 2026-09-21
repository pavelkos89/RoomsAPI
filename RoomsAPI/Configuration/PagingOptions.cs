namespace RoomsAPI.Configuration;

public class PagingOptions
{
	public const string SectionName = "Paging";

	public int DefaultPageSize { get; set; } = 50;

	public int MaxPageSize { get; set; } = 100;
}
