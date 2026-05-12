using System;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken=default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c=>c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetWithProductsAsync(int id, CancellationToken cancellationToken=default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c=>c.Id==id && c.IsActive)
            .Include(c=>c.Products!.Where(p=>p.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetWithProductsAsync(CancellationToken cancellationToken=default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c=>c.IsActive)
            .Include(c=>c.Products!.Where(p=>p.IsActive))
            .ToListAsync(cancellationToken);
    }
}
