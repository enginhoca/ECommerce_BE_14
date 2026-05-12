using System;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;

namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<Result<PagedResponseDto<ProductDto>>> GetProductsAsync(ProductFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken);

    Task<Result<ProductDto>> GetProductByIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<ProductDto>> CreateProductAsync(ProductCreateDto dto, CancellationToken cancellationToken);

    Task<Result<ProductDto>> UpdateProductAsync(int id, ProductUpdateDto dto,CancellationToken cancellationToken);

    Task<Result<bool>> DeleteProductAsync(int id, CancellationToken cancellationToken);

    Task<Result<string>> UploadImageAsync(int productId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken);
}
