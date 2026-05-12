using System;
using ECommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerce.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ECommerceDbContext _context;
    protected readonly DbSet<T> _dbSet;
    public Repository(ECommerceDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken=default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;

    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken=default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken=default)
    {
        return
            predicate is null
                ? await _dbSet.CountAsync(cancellationToken)
                : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken=default)
    {
        return await _dbSet.AnyAsync(predicate,cancellationToken );
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken=default)
    {
        return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken=default)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken=default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }


    // public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<T?> GetByIdAsync(int id, bool ignoreIsDeleted=true, CancellationToken cancellationToken=default)
    {
        if(!ignoreIsDeleted)
        {
            _dbSet.IgnoreQueryFilters();
        }
        return await _dbSet.FindAsync(id, cancellationToken);
    }


    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsNoTracking().AsQueryable();
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }
}
