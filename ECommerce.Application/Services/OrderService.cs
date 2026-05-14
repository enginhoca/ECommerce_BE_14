using System;
using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> CreateOrderAsync(OrderCreateDto dto, string customerId, CancellationToken cancellationToken)
    {
        // Transaction'ı başlat
        await _uow.BeginTransactionAsync(cancellationToken);
        try
        {
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;
            foreach (var itemDto in dto.Items)
            {
                var product = await _uow.Products.GetByIdAsync((int)itemDto.ProductId!, cancellationToken:cancellationToken);
                if (product is null || !product.IsActive)
                {
                    return Result<OrderDto>.NotFound($"{itemDto.ProductId} ID'li ürün bulunamadı!");
                }

                if (product.Stock < itemDto.Quantity)
                {
                    return Result<OrderDto>.ValidationFailure(
                        $"{product.Name} ürününde yeterli stok yok. Mevcut Stok: {product.Stock}, İstenen: {itemDto.Quantity}"
                    );
                }

                product.Stock -= (int)itemDto.Quantity!;
                product.UpdatedDate = DateTime.UtcNow;
                _uow.Products.Update(product);

                var itemTotal = product.Price * itemDto.Quantity;
                totalAmount += (decimal)itemTotal!;

                orderItems.Add(new OrderItem
                {
                    ProductId = (int)itemDto.ProductId,
                    Quantity = (int)itemDto.Quantity,
                    UnitPrice = (decimal)product.Price!,
                    TotalPrice = (decimal)itemTotal
                });
            }

            var orderNumber = await _uow.Orders.GenerateOrderNumberAsync(cancellationToken);

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                FinalAmount = totalAmount,
                Status = OrderStatus.Pending,
                ShippingAddress = dto.ShippingAddress,
                OrderItems = orderItems
            };
            await _uow.Orders.AddAsync(order, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            await _uow.CommitTransactionAsync(cancellationToken);

            var createdOrder = await _uow.Orders.GetByIdWithItemsAsync(order.Id, cancellationToken:cancellationToken);
            var createdOrderDto = _mapper.Map<OrderDto>(createdOrder);
            return Result<OrderDto>.Success(createdOrderDto);
        }
        catch (Exception)
        {
            await _uow.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<PagedResponseDto<OrderDto>>> GetAllOrdersAsync(PaginationDto pagination, CancellationToken cancellationToken)
    {
        var allOrders = await _uow.Orders.GetOrdersWithItemsAsync(cancellationToken);
        var totalCount = allOrders.Count();

        var items = allOrders
            .OrderByDescending(o => o.OrderDate)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize);

        var pagedResponseDto = new PagedResponseDto<OrderDto>
        {
            Items = _mapper.Map<IEnumerable<OrderDto>>(items),
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
        return Result<PagedResponseDto<OrderDto>>.Success(pagedResponseDto);
    }

    public async Task<Result<IEnumerable<OrderDto>>> GetCustomerOrdersAsync(string customerId, CancellationToken cancellationToken)
    {
        var orders = await _uow.Orders.GetByCustomerIdAsync(customerId, cancellationToken);
        var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
        return Result<IEnumerable<OrderDto>>.Success(orderDtos);
    }

    public async Task<Result<OrderDto>> GetOrderByIdAsync(int id, CancellationToken cancellationToken)
    {
        var order = await _uow.Orders.GetByIdWithItemsAsync(id, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.NotFound($"{id} ID'li sipariş bulunamdı!");
        }
        var dto = _mapper.Map<OrderDto>(order);
        return Result<OrderDto>.Success(dto);
    }

    public async Task<Result<OrderDto>> UpdateOrderStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken)
    {
        var order = await _uow.Orders.GetByIdAsync(id, cancellationToken:cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.NotFound($"{id} ID'li sipariş bulunamadı!");
        }
        if (order.Status == OrderStatus.Cancelled)
        {
            return Result<OrderDto>.ValidationFailure("İptal edilmiş sipariş güncellenemz!");
        }

        if (order.Status == OrderStatus.Delivered && status == OrderStatus.Cancelled)
        {
            return Result<OrderDto>.ValidationFailure("Teslim edilmiş sipariş iptal edilemez!");
        }

        order.Status = status;
        order.UpdatedDate = DateTime.UtcNow;
        _uow.Orders.Update(order);
        await _uow.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<OrderDto>(order);
        return Result<OrderDto>.Success(dto);
    }
}
