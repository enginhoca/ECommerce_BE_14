using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Order;

public class OrderItemCreateDto
{
    [Required(ErrorMessage ="Ürün seçimi zorunludur.")]
    [Range(1,int.MaxValue,ErrorMessage ="Geçerli bir ürün seçin!")]
    public int? ProductId { get; set; }

    [Required(ErrorMessage ="Miktar zorunludur.")]
    [Range(1,100,ErrorMessage ="Miktar 1-100 arasında olmalıdır!")]
    public int? Quantity { get; set; }
}
