using Microsoft.EntityFrameworkCore;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.DAL.Extensions;

public static class QueryableExtensions
{
    public static async Task<PaginatedList<TItem>> ToPagedListAsync<TItem>(
        this IQueryable<TItem> source,
        PaginationParameters? paginationParameters,
        CancellationToken cancellationToken)
    {
        var totalCount = await source.CountAsync(cancellationToken);
        
        var pageNumber = paginationParameters?.PageNumber ?? 1;
        var pageSize = paginationParameters?.PageSize ?? totalCount;

        var appliedPageSize = pageSize > 0
            ? pageSize
            : 10;

        var items = await source
            .Skip((pageNumber - 1) * appliedPageSize)
            .Take(appliedPageSize)
            .ToListAsync(cancellationToken);

        return items.ToPagedList(pageNumber, pageSize, totalCount);
    }
}
