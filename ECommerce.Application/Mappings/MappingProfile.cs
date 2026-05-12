using System;
using AutoMapper;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.Product;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Category
        CreateMap<Category, CategoryDto>()
            .ForMember(dest=>dest.ProductCount, 
                opt=> opt.MapFrom(src=>src.Products != null ? src.Products.Count : 0));

        CreateMap<CategoryCreateDto, Category>();
        CreateMap<CategoryUpdateDto, Category>();

        // Product
        CreateMap<Product, ProductDto>()
            .ForMember(dest=>dest.CategoryName,
                opt=>opt.MapFrom(src=>src.Category != null ? src.Category.Name : "Kategori bilgisi yok!"));

        CreateMap<ProductCreateDto, Product>();
        CreateMap<ProductUpdateDto, Product>();

        // Order
        CreateMap<Order, OrderDto>();

        // OrderItem
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest=>dest.ProductName,
                opt=>opt.MapFrom(src=>src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest=>dest.ProductImageUrl,
                opt=>opt.MapFrom(src=>src.Product != null ? src.Product.ImageUrl : null));


        

    }
}
