using System;
using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.DTOs.Order;

public class OrderStatusUpdateDto
{
    [Required(ErrorMessage ="Durum zorunludur.")]
    public OrderStatus Status { get; set; }
}
