using System;
using AutoMapper;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileUploadService _fileUploadService;

    public ProductService(IUnitOfWork uow, IMapper mapper, IFileUploadService fileUploadService)
    {
        _uow = uow;
        _mapper = mapper;
        _fileUploadService = fileUploadService;
    }

    public async Task<Result<ProductDto>> CreateProductAsync(ProductCreateDto dto, CancellationToken cancellationToken)
    {
        var category = await _uow.Categories.GetByIdAsync(dto.CategoryId, cancellationToken:cancellationToken);
        if (category is null)
        {
            return Result<ProductDto>.NotFound($"{dto.CategoryId} ID'li kategori bulunamadı, bu yüzden ürün kaydedilemedi!");
        }

        var product = _mapper.Map<Product>(dto);
        product.CreatedDate = DateTime.UtcNow;
        product.IsActive = false;
        await _uow.Products.AddAsync(product,cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        product.Category = category;
        var productDto = _mapper.Map<ProductDto>(product);

        return Result<ProductDto>.Success(productDto);
    }

    public async Task<Result<bool>> DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _uow.Products.GetByIdAsync(id, false, cancellationToken);
        if (product is null)
        {
            return Result<bool>.NotFound($"{id} ID'li ürün bulunamadığı için silinemedi!");
        }
        product.IsDeleted = !product.IsDeleted;
        product.UpdatedDate = DateTime.UtcNow;
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }



    public async Task<Result<ProductDto>> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await _uow.Products.GetWithCategoryAsync(id, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.NotFound($"{id} ID'li ürün bulunamadı!");
        }
        var dto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(dto);
    }

    public async Task<Result<PagedResponseDto<ProductDto>>> GetProductsAsync(ProductFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _uow.Products.GetPagedProductsAsync(
            searchTerm: filter.SearchTerm,
            categoryId: filter.CategoryId,
            minPrice: filter.MinPrice,
            maxPrice: filter.MaxPrice,
            isActive: filter.IsActive,
            isDeleted: filter.IsDeleted,
            ignoreIsDeleted: filter.IgnoreIsDeleted,
            sortBy: filter.SortBy,
            sortDescending: filter.SortDescending,
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize, 
            cancellationToken:cancellationToken
        );
        
        var dtos = _mapper.Map<IEnumerable<ProductDto>>(items);

        var pagedResponseDto = new PagedResponseDto<ProductDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };

        return Result<PagedResponseDto<ProductDto>>.Success(pagedResponseDto);

    }

    public async Task<Result<ProductDto>> UpdateProductAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken)
    {
        var product = await _uow.Products.GetByIdAsync(id, cancellationToken:cancellationToken);
        if (product is null || !product.IsActive)
        {
            return Result<ProductDto>.NotFound($"{id} ID'li ürün bulunamadığı için güncellenemedi!");
        }

        if (dto.CategoryId != product.CategoryId)
        {
            var category = await _uow.Categories.GetByIdAsync(dto.CategoryId, cancellationToken:cancellationToken);
            if (category is null)
            {
                return Result<ProductDto>.NotFound($"{dto.CategoryId} ID'li kategori bulunamadığı için ürün güncellenemedi!");
            }
        }

        _mapper.Map(dto, product);
        product.UpdatedDate = DateTime.UtcNow;

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(cancellationToken);

        var productDto = _mapper.Map<ProductDto>(product);
        return Result<ProductDto>.Success(productDto);
    }

    public async Task<Result<string>> UploadImageAsync(int productId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var product = await _uow.Products.GetByIdAsync(productId, cancellationToken:cancellationToken);
        if (product is null)
        {
            return Result<string>.NotFound($"{productId} ID'li ürün bulunamadığı için resim yüklenemedi!");
        }
        var uploadResult = await _fileUploadService.UploadImageAsync(fileStream, fileName, contentType, cancellationToken);
        if (!uploadResult.IsSuccess)
        {
            return Result<string>.Unexpected(uploadResult.Error!);
        }
        product.ImageUrl = uploadResult.Value;
        product.UpdatedDate = DateTime.UtcNow;
        product.IsActive = true;
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(cancellationToken);
        return Result<string>.Success(uploadResult.Value!);
    }
}
