using System;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetActiveAsync(CancellationToken cancellationToken=default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedProductsAsync(
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
    )
    {
        var query = _dbSet.Include(p => p.Category).AsQueryable();

        // Filtreleme
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name!.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm)));
        }
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (isDeleted.HasValue)
        {
            if (!ignoreIsDeleted) query.IgnoreQueryFilters();
            query = query.Where(p => p.IsDeleted == isDeleted.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sıralama
        query = sortBy?.ToLowerInvariant() switch
        {
            "price" => sortDescending.HasValue && sortDescending==true ? query.OrderByDescending(p=>p.Price) : query.OrderBy(p=>p.Price), 
            "stock" => sortDescending.HasValue && sortDescending==true ? query.OrderByDescending(p=>p.Stock) : query.OrderBy(p=>p.Stock), 
            "created" => sortDescending.HasValue && sortDescending==true ? query.OrderByDescending(p=>p.CreatedDate) : query.OrderBy(p=>p.CreatedDate),
            _         => sortDescending.HasValue && sortDescending==true ? query.OrderByDescending(p=>p.Name) : query.OrderBy(p=>p.Name) 
        };

        // Sayfalama
        var items = await query
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount); // touple
    }

    // 76 adet ürün var.
    // Bir sayfada 10 ürün göster. Bana 2.sayfadaki ürünleri getir.
    // pageSize=10, pageNumber=5
    // Skip((5-1)*10)=40

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken=default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Include(p => p.Category)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetWithCategoryAsync(int id, CancellationToken cancellationToken=default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p=>p.Id==id && p.IsActive)
            .Include(c=>c.Category)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
