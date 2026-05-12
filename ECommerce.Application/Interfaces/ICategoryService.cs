using System;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken);
    
    Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<CategoryDto>> CreateCategoryAsync(CategoryCreateDto dto, CancellationToken cancellationToken);

    Task<Result<CategoryDto>> UpdateCategoryAsync(int id, CategoryUpdateDto dto, CancellationToken cancellationToken);

    Task<Result<object>> DeleteCategoryAsync(int id, CancellationToken cancellationToken);
}
