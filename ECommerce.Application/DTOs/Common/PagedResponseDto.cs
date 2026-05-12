using System;

namespace ECommerce.Application.DTOs.Common;

public class PagedResponseDto<T>
{
    public IEnumerable<T> Items { get; set; }=[];
    public int TotalCount { get; set; } // 21
    public int PageNumber { get; set; }
    public int PageSize { get; set; } // 10
    public int TotalPages => (int)Math.Ceiling(TotalCount/(double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber<TotalPages;
}
