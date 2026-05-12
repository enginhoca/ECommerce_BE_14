using System;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Quantity).IsRequired();

        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)").HasPrecision(18, 2);

        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)").HasPrecision(18, 2);

        builder.HasIndex(x => x.OrderId).HasDatabaseName("IX_OrderItems_OrderId");

        builder.HasIndex(x => x.ProductId).HasDatabaseName("IX_OrderItems_ProductId");

        builder.HasIndex(x => new { x.OrderId, x.ProductId }).IsUnique().HasDatabaseName("IX_OrderItems_OrderProduct");

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);


    }
}
