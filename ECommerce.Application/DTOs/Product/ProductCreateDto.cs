using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Product;

public class ProductCreateDto
{
    [Required(ErrorMessage ="Ürün adı zorunludur.")]
    [StringLength(200, MinimumLength =3,ErrorMessage ="Ürün adı 3-200 karakter olabilir.")]
    public string Name { get; set; }=string.Empty;

    [StringLength(2000,ErrorMessage ="Ürün açıklaması 2000 karakterden fazla olamaz.")]
    public string? Description { get; set; }

    [Required(ErrorMessage ="Fiyat zorunludur.")]
    [Range(0.01, double.MaxValue,ErrorMessage ="Fiyat 0'dan büyük olmalıdır.")]
    public decimal? Price { get; set; }

    
    [Range(0, int.MaxValue, ErrorMessage ="Stok negatif olamaz!")]
    public int Stock { get; set; }

    [Required(ErrorMessage ="Kategori seçimi zorunludur!")]
    [Range(1, int.MaxValue, ErrorMessage ="Geçerli bir kategori seçiniz!")]
    public int CategoryId { get; set; }

}
