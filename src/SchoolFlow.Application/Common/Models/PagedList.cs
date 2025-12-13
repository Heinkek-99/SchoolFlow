namespace SchoolFlow.Application.Common.Models;

/// <summary>
/// Représente une liste paginée de résultats
/// </summary>
public class PagedList<T>
{
    public List<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedList(List<T> Items, int TotalCount, int PageNumber, int PageSize)
    {
        this.Items = Items;
        this.TotalCount = TotalCount;
        this.PageNumber = PageNumber;
        this.PageSize = PageSize;
    }

    public static PagedList<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PagedList<T>(items, totalCount, pageNumber, pageSize);
    }
}