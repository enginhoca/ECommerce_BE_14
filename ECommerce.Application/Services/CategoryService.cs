using System;
using AutoMapper;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CategoriesCacheDuration = TimeSpan.FromMinutes(2);
    public const string CategoriesListCacheKey = "categories:active-with-products";

    public CategoryService(IUnitOfWork uow, IMapper mapper, IMemoryCache cache)
    {
        _uow = uow;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<Result<CategoryDto>> CreateCategoryAsync(CategoryCreateDto dto, CancellationToken cancellationToken)
    {
        var existing = await _uow.Categories
            .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower(), cancellationToken);
        if (existing is not null)
        {
            return Result<CategoryDto>.Conflict($"'{dto.Name}' adında bir kategori, zaten mevcut!");
        }

        var category = _mapper.Map<Category>(dto);
        category.CreatedDate = DateTime.UtcNow;
        category.IsActive = true;

        await _uow.Categories.AddAsync(category, cancellationToken);
        
        await _uow.SaveChangesAsync(cancellationToken);

        var newCategory = await _uow.Categories.GetWithProductsAsync(category.Id, cancellationToken);
        var categoryDto = _mapper.Map<CategoryDto>(newCategory);

        return Result<CategoryDto>.Success(categoryDto);

    }

    public async Task<Result<object>> DeleteCategoryAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _uow.Categories.GetWithProductsAsync(id, cancellationToken);
        if (category is null)
        {
            return Result<object>.NotFound($"{id} ID'li kategori bulunamadığı için silinemedi!");
        }

        if(category.Products!.Count != 0)
        {
            return Result<object>.Conflict($"Bu kategoride {category.Products.Count} adet ürün bulunuyor! Silmeden önce ürünleri başka bir kategoriye taşımayı deneyebilirsiniz.");
        }

        _uow.Categories.Delete(category);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result<object>.Success(null!);

    }

    public async Task<Result<IEnumerable<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        if(_cache.TryGetValue(CategoriesListCacheKey, out IEnumerable<CategoryDto>?cached) && cached is not null)
        {
            return Result<IEnumerable<CategoryDto>>.Success(cached);
        } 
        var categories = await _uow.Categories.GetWithProductsAsync(cancellationToken);
        var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

        _cache.Set(CategoriesListCacheKey, dtos, new MemoryCacheEntryOptions{AbsoluteExpirationRelativeToNow=CategoriesCacheDuration});
        return Result<IEnumerable<CategoryDto>>.Success(dtos);
    }

    public async Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _uow.Categories.GetWithProductsAsync(id, cancellationToken);
        if (category is null)
        {
            return Result<CategoryDto>.NotFound($"{id} ID'li kategori bulunamadı!");
        }
        var dto = _mapper.Map<CategoryDto>(category);
        return Result<CategoryDto>.Success(dto);
    }

    public async Task<Result<CategoryDto>> UpdateCategoryAsync(int id, CategoryUpdateDto dto, CancellationToken cancellationToken)
    {
        var category = await _uow.Categories.GetByIdAsync(id, cancellationToken:cancellationToken);
        if (category is null)
        {
            return Result<CategoryDto>.NotFound($"{id} ID'li kategori bulunamadığı için güncellenemedi!");
        }

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IsActive = dto.IsActive;

        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync(cancellationToken);

        var categoryDto = _mapper.Map<CategoryDto>(category);

        return Result<CategoryDto>.Success(categoryDto);
    }
}
