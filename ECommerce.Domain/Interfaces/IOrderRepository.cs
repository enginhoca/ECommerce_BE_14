using System;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken=default);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken=default);
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken=default);
}
