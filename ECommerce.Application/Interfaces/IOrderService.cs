using System;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Order;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<Result<OrderDto>> CreateOrderAsync(OrderCreateDto dto, string customerId, CancellationToken cancellationToken);
    
    Task<Result<OrderDto>> GetOrderByIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<IEnumerable<OrderDto>>> GetCustomerOrdersAsync(string customerId, CancellationToken cancellationToken);

    Task<Result<PagedResponseDto<OrderDto>>> GetAllOrdersAsync(PaginationDto pagination, CancellationToken cancellationToken);

    Task<Result<OrderDto>> UpdateOrderStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken);
}
