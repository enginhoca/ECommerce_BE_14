using System;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(500);

        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasPrecision(18, 2);

        builder.Property(x => x.Stock).HasDefaultValue(0);

        builder.Property(x => x.ImageUrl).HasMaxLength(500);

        builder.Property(x => x.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.IsActive).HasDefaultValue(true);


        builder.HasIndex(x => x.Name).HasDatabaseName("IX_Products_Name");

        builder.HasIndex(x => x.CategoryId).HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(x => x.Price).HasDatabaseName("IX_Products_Price");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);

       
    }
}
