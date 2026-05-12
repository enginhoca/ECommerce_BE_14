using System;
using System.ComponentModel;


namespace ECommerce.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedDate { get; set; } 
    public DateTime UpdatedDate { get; set; } 
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;


    public virtual Category? Category { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }=[];
}
