using System;
using System.Security.AccessControl;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ECommerceDbContext context,
        UserManager<AppIdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        await context.Database.MigrateAsync();
        await SeedRolesAsync(roleManager, logger);
        await SeedAdminUserAsync(userManager, logger);
        await SeedCategoriesAsync(context, logger);
        await SeedProductsAsync(context, logger);
        await SeedOrdersAsync(context, logger);
        await SeedOrderItemsAsync(context, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = ["Admin", "Manager", "Customer"];

        foreach (var role in roles)
        {
            if(!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation($"Rol oluşturuldu: {role}");
            }
        } 
    }

    private static async Task SeedAdminUserAsync(UserManager<AppIdentityUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@example.com";
        if(await userManager.FindByEmailAsync(adminEmail) is not null)
        {
            return;
        }
        var admin = new AppIdentityUser
        {
            Id="e60f92e4-2e9c-4c79-a826-1304870622f7",
            UserName="admin",
            Email=adminEmail,
            FirstName="Admin",
            LastName="User",
            EmailConfirmed=true,
            CreatedDate=DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if(result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            logger.LogInformation($"Admin kullanıcısı oluşturuldu: {adminEmail}");
        }
    }

    private static async Task SeedCategoriesAsync(ECommerceDbContext context, ILogger logger)
    {
        if(await context.Categories.AnyAsync()) return;
        // Id verme: PostgreSQL identity sırasını elle ilerletmez; sonraki INSERT çakışır (23505).
        // Boş tabloya sırayla eklendiği için Id'ler 1..4 olur; ürün seed'i CategoryId ile uyumludur.
        List<Category> categories = [
            new Category { Name = "Elektronik", Description = "Elektronik ürünler", IsActive = true },
            new Category { Name = "Giyim", Description = "Giyim ürünleri", IsActive = true },
            new Category { Name = "Kitap", Description = "Kitap ve dergi", IsActive = true },
            new Category { Name = "Ev & Yaşam", Description = "Ev ve yaşam ürünleri", IsActive = true }
        ];
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        logger.LogInformation("Örnek kategori bilgileri veri tabanına eklendi.");
    }

    private static async Task SeedProductsAsync(ECommerceDbContext context, ILogger logger)
    {
        if(await context.Products.AnyAsync()) return;
        List<Product> products = [
            new Product { Name = "Laptop", Description = "Gaming laptop", Price = 15000.00m, Stock = 10, CategoryId = 1, IsActive = true },
            new Product { Name = "Mouse", Description = "Kablosuz mouse", Price = 250.00m, Stock = 50, CategoryId = 1, IsActive = true },
            new Product { Name = "Klavye", Description = "Mekanik klavye", Price = 1200.00m, Stock = 30, CategoryId = 1, IsActive = true },
            new Product { Name = "Monitör", Description = "27 inç monitör", Price = 8500.00m, Stock = 15, CategoryId = 1, IsActive = true },
            new Product { Name = "Harici Disk", Description = "1TB SSD", Price = 2200.00m, Stock = 40, CategoryId = 1, IsActive = true },
            new Product { Name = "USB Hub", Description = "7 portlu USB 3.0", Price = 180.00m, Stock = 100, CategoryId = 1, IsActive = true },
            new Product { Name = "Tişört", Description = "Pamuklu tişört", Price = 149.00m, Stock = 200, CategoryId = 2, IsActive = true },
            new Product { Name = "Gömlek", Description = "Resmi gömlek", Price = 299.00m, Stock = 80, CategoryId = 2, IsActive = true },
            new Product { Name = "Pantolon", Description = "Kot pantolon", Price = 399.00m, Stock = 60, CategoryId = 2, IsActive = true },
            new Product { Name = "Çeket", Description = "Kışlık çeket", Price = 899.00m, Stock = 25, CategoryId = 2, IsActive = true },
            new Product { Name = "Mont", Description = "Rüzgarlık mont", Price = 549.00m, Stock = 35, CategoryId = 2, IsActive = true },
            new Product { Name = "Roman", Description = "Bestseller roman", Price = 45.00m, Stock = 150, CategoryId = 3, IsActive = true },
            new Product { Name = "Teknik Kitap", Description = "Programlama kitabı", Price = 120.00m, Stock = 70, CategoryId = 3, IsActive = true },
            new Product { Name = "Çocuk Kitabı", Description = "Resimli çocuk kitabı", Price = 35.00m, Stock = 120, CategoryId = 3, IsActive = true },
            new Product { Name = "Dergi", Description = "Aylık dergi", Price = 25.00m, Stock = 300, CategoryId = 3, IsActive = true },
            new Product { Name = "Halı", Description = "Salon halısı", Price = 1200.00m, Stock = 20, CategoryId = 4, IsActive = true },
            new Product { Name = "Perde", Description = "Siyah out perde", Price = 450.00m, Stock = 45, CategoryId = 4, IsActive = true },
            new Product { Name = "Aydınlatma", Description = "LED masa lambası", Price = 280.00m, Stock = 55, CategoryId = 4, IsActive = true }
        ];
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
        logger.LogInformation("Ürünler eklendi.");
    }

    private static async Task SeedOrdersAsync(ECommerceDbContext context, ILogger logger)
    {
        if(await context.Orders.AnyAsync()) return;

        List<Order> orders = [
            new Order { OrderNumber = "ORD-001", CustomerId = "e60f92e4-2e9c-4c79-a826-1304870622f7", OrderDate = new DateTime(2026,1,22,0,0,0,DateTimeKind.Utc), TotalAmount = 16700.00m, FinalAmount = 16700.00m, Status = OrderStatus.Delivered },
            new Order { OrderNumber = "ORD-002", CustomerId = "e60f92e4-2e9c-4c79-a826-1304870622f7", OrderDate = new DateTime(2026,1,27,0,0,0,DateTimeKind.Utc), TotalAmount = 10700.00m, FinalAmount = 10700.00m, Status = OrderStatus.Shipped },
            new Order { OrderNumber = "ORD-003", CustomerId = "e60f92e4-2e9c-4c79-a826-1304870622f7", OrderDate = new DateTime(2026,2,15,0,0,0,DateTimeKind.Utc), TotalAmount = 1002.00m, FinalAmount = 1002.00m, Status = OrderStatus.Pending},
            new Order { OrderNumber = "ORD-004", CustomerId = "e60f92e4-2e9c-4c79-a826-1304870622f7", OrderDate = new DateTime(2026,2,17,0,0,0,DateTimeKind.Utc), TotalAmount = 45.00m, FinalAmount = 45.00m, Status = OrderStatus.Pending }
        ];
        await context.Orders.AddRangeAsync(orders);
        await context.SaveChangesAsync();
        logger.LogInformation("Siparişler eklendi.");
    }

    private static async Task SeedOrderItemsAsync(ECommerceDbContext context, ILogger logger)
    {
        if(await context.OrderItems.AnyAsync()) return;

        List<OrderItem> orderItems = [
            new OrderItem { OrderId = 1, ProductId = 1, Quantity = 1, UnitPrice = 15000.00m, TotalPrice = 15000.00m },
            new OrderItem { OrderId = 1, ProductId = 2, Quantity = 2, UnitPrice = 250.00m, TotalPrice = 500.00m },
            new OrderItem { OrderId = 1, ProductId = 3, Quantity = 1, UnitPrice = 1200.00m, TotalPrice = 1200.00m },
            new OrderItem { OrderId = 2, ProductId = 4, Quantity = 1, UnitPrice = 8500.00m, TotalPrice = 8500.00m },
            new OrderItem { OrderId = 2, ProductId = 5, Quantity = 1, UnitPrice = 2200.00m, TotalPrice = 2200.00m },
            new OrderItem { OrderId = 3, ProductId = 6, Quantity = 2, UnitPrice = 180.00m, TotalPrice = 360.00m },
            new OrderItem { OrderId = 3, ProductId = 7, Quantity = 2, UnitPrice = 149.00m, TotalPrice = 298.00m },
            new OrderItem { OrderId = 3, ProductId = 8, Quantity = 1, UnitPrice = 299.00m, TotalPrice = 299.00m },
            new OrderItem { OrderId = 3, ProductId = 12, Quantity = 1, UnitPrice = 45.00m, TotalPrice = 45.00m },
            new OrderItem { OrderId = 4, ProductId = 12, Quantity = 1, UnitPrice = 45.00m, TotalPrice = 45.00m }
        ];
        await context.OrderItems.AddRangeAsync(orderItems);
        await context.SaveChangesAsync();
        logger.LogInformation("Sipariş kalemleri eklendi.");
    }

}