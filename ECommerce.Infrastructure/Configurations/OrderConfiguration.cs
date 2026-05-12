using System;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x=>x.Id);

        builder.Property(x=>x.Id).ValueGeneratedOnAdd();

        builder.Property(x=>x.OrderNumber).IsRequired().HasMaxLength(50);

        builder.Property(x=>x.OrderDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x=>x.TotalAmount).HasColumnType("decimal(18,2)").HasPrecision(18,2);

        builder.Property(x=>x.FinalAmount).HasColumnType("decimal(18,2)").HasPrecision(18,2);

        builder.HasIndex(x=>x.OrderNumber).IsUnique().HasDatabaseName("IX_Orders_OrderNumber");

        builder.HasIndex(x=>x.CustomerId).HasDatabaseName("IX_Orders_CustomerId");

        builder.HasIndex(x=>x.OrderDate).HasDatabaseName("IX_Orders_OrderDate");

        builder.HasMany(o=>o.OrderItems)
            .WithOne(oi=>oi.Order)
            .HasForeignKey(oi=>oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);


    }
}
