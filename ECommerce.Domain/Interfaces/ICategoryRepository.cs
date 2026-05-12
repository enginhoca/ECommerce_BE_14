using System;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

public interface ICategoryRepository:IRepository<Category>
{
    Task<Category?> GetWithProductsAsync(int id, CancellationToken cancellationToken=default);
    Task<IEnumerable<Category>> GetWithProductsAsync(CancellationToken cancellationToken=default);
    Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken=default);
}
