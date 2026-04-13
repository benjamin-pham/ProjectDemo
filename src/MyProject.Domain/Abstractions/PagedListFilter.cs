namespace MyProject.Domain.Abstractions;

public record PagedListFilter
{
    private const int DefaultPageNumber = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    private int _pageNumber = DefaultPageNumber;
    private int _pageSize = DefaultPageSize;

    public int? PageNumber
    {
        get; set;
    }

    public int? PageSize
    {
        get; set;
    }

    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
    public string? SearchTerm { get; init; }
}
