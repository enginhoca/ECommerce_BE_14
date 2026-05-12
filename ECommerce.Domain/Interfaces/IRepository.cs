using System;
using System.Linq.Expressions;

namespace ECommerce.Domain.Interfaces;
// T-> Product ya da Category ya da Order olabilir!!!
public interface IRepository<T> where T: class
{
    Task<T?> GetByIdAsync(int id, bool ignoreIsDeleted=true, CancellationToken cancellationToken=default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken=default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken=default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken=default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken=default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken=default);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken=default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate=null, CancellationToken cancellationToken=default);
    IQueryable<T> GetQueryable();
}