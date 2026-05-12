using System;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x=>x.Id);

        builder.Property(x=>x.Id).ValueGeneratedOnAdd();

        builder.Property(x=>x.Name).IsRequired().HasMaxLength(100);
        
        builder.Property(x=>x.Description).HasMaxLength(500);

        builder.Property(x=>x.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        builder.Property(x=>x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x=>x.Name).HasDatabaseName("IX_Categories_Name");

        builder.HasMany(c=>c.Products)  // Category'de Products koleksiyonu
            .WithOne(p=>p.Category)     // Product'ta Category navigation propertysi
            .HasForeignKey(p=>p.CategoryId) //Foreign Key
            .OnDelete(DeleteBehavior.Restrict); // Kategori, en az bir ürüne sahipse kategori silinemez


        

    }
}
