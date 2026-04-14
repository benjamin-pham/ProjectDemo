namespace MyProject.Domain.Abstractions;

public record PagedListFilter
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;

    public int? PageNumber { get; set; } = DefaultPageNumber;
    public int? PageSize { get; set; } = DefaultPageSize;
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
    public string? SearchTerm { get; init; }
}
