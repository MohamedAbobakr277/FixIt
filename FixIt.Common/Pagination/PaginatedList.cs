using System;
using System.Collections.Generic;

namespace FixIt.Common.Pagination;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public int PageSize { get; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        PageSize = pageSize;
        Items = items;
    }

    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async System.Threading.Tasks.Task<PaginatedList<T>> CreateAsync(System.Linq.IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(source);
        var items = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(source.Skip((pageIndex - 1) * pageSize).Take(pageSize));
        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }
}
