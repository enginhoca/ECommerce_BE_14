using System;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? CustomerFullName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? ShippingAddress { get; set; } 
    public List<OrderItemDto> OrderItems { get; set; } = [];
}
