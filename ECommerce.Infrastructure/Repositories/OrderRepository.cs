using System;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ECommerceDbContext context) : base(context)
    {
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken=default)
    {
        // ORD-20260105-0045
        var today = DateTime.UtcNow;
        var prefix = $"ORD-{today:yyyyMMdd}";
        var todayCount = await _dbSet
            .CountAsync(o=>o.OrderNumber.StartsWith(prefix));
        var orderNumber = $"{prefix}-{(todayCount+1):D4}";
        return orderNumber;
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken=default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o=>o.CustomerId==customerId)
            .Include(o=>o.OrderItems)
                .ThenInclude(oi=>oi.Product)
            .OrderByDescending(o=>o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken=default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o=>o.Id==id)
            .Include(o=>o.OrderItems)
                .ThenInclude(oi=>oi.Product)
                    .ThenInclude(p=>p!.Category)
                        .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
