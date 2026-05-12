using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Order;

public class OrderCreateDto
{
    [Required(ErrorMessage ="Adres zorunludur.")]
    [StringLength(500, ErrorMessage ="Adres en fazla 500 karakter olabilir.")]
    public string? ShippingAddress { get; set; }

    [Required(ErrorMessage ="En az bir ürün seçilmelidir.")]
    [MinLength(1,ErrorMessage ="En az bir ürün seçmelisiniz")]
    public List<OrderItemCreateDto> Items { get; set; }=[];
}
