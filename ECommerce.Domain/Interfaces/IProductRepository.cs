using System;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken=default);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedProductsAsync(
        string? searchTerm,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isActive,
        bool? isDeleted,
        string? sortBy,
        bool? sortDescending,
        int pageNumber,
        int pageSize,
        bool ignoreIsDeleted = true,
        CancellationToken cancellationToken=default
    );
    Task<IEnumerable<Product>> GetActiveAsync(CancellationToken cancellationToken=default);
    Task<Product?> GetWithCategoryAsync(int id, CancellationToken cancellationToken=default);
}
