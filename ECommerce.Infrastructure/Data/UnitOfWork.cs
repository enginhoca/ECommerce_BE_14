using System;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    private IDbContextTransaction _transaction;
    public UnitOfWork(ECommerceDbContext context)
    {
        _context = context;
        Products = new ProductRepository(_context);
        Categories = new CategoryRepository(_context);
        Orders = new OrderRepository(_context);
        _transaction=null!;
    }
    public ICategoryRepository Categories {get; private set;}

    public IProductRepository Products {get; private set;}

    public IOrderRepository Orders {get; private set;}

    public async Task BeginTransactionAsync(CancellationToken cancellationToken=default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken=default)
    {
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken=default)
    {
        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken=default)
    {
        int result = await _context.SaveChangesAsync(cancellationToken);
        return result;
    }
}
