using System.ComponentModel;

namespace MyProject.Domain.Abstractions;

public record PagedListFilter
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;
    [DefaultValue(DefaultPageNumber)]
    public int? PageNumber { get; set; } = DefaultPageNumber;
    [DefaultValue(DefaultPageSize)]
    public int? PageSize { get; set; } = DefaultPageSize;
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
    public string? SearchTerm { get; init; }
}