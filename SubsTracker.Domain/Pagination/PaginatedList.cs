namespace SubsTracker.Domain.Pagination;

public record PaginatedList<TItem>(List<TItem> Items, int PageNumber, int PageSize, int PageCount, int TotalCount)
{
    public bool HasPreviousPage => PageNumber > 1;
    
    public bool HasNextPage => PageNumber < TotalCount;
    
    public PaginatedList<TTarget> MapToPage<TTarget>(Func<TItem, TTarget> mapFunc)
    {
        var mappedItems = Items.Select(mapFunc).ToList();
        return new PaginatedList<TTarget>(mappedItems, PageNumber, PageSize, PageCount, TotalCount);
    }
}
