using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Category;

public class CategoryCreateDto
{
    [Required(ErrorMessage = "Kategori adı zorunludur!CATEGORYCREATEDTO")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Kategori adı uzunluğu 3-100 karakter arasında olmalıdır.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama uzunluğu en fazla 500 karakter olmalıdır!")]
    public string? Description { get; set; }
}
