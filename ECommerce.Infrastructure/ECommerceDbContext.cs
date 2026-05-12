using System;
using System.Reflection;
using ECommerce.Infrastructure.Configurations;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ECommerce.Infrastructure.Identity;

namespace ECommerce.Infrastructure;

public class ECommerceDbContext : IdentityDbContext<AppIdentityUser>
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options):base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);// Identity tablolarının yapılandırılmasını sağlayacak
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());   
    }
}
