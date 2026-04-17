namespace ProjectTemplate.Domain.Abstractions;

public sealed class PagedList<TItem>
{
    public PagedList(List<TItem> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalItems = totalCount;
    }

    public List<TItem> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalItems { get; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
