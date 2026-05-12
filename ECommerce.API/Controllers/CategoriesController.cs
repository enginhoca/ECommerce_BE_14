using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Produces("application/json")]
    public class CategoriesController : ApiControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll( CancellationToken cancellationToken) => FromResult(await _categoryService.GetCategoriesAsync(cancellationToken));


        [HttpGet("{id:int}", Name = nameof(GetCategoryById))]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryById(int id,  CancellationToken cancellationToken) => FromResult(await _categoryService.GetCategoryByIdAsync(id, cancellationToken));


        [HttpPost]
        [Authorize(Roles ="Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>),StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto,  CancellationToken cancellationToken)
        {
            var result = await _categoryService.CreateCategoryAsync(dto, cancellationToken);
            if(!result.IsSuccess)
                return FromResult(result);
            return CreatedAtAction(nameof(GetCategoryById), new {id=result.Value!.Id}, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>),StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto,  CancellationToken cancellationToken) => FromResult(await _categoryService.UpdateCategoryAsync(id, dto, cancellationToken));

        [HttpDelete("{id:int}")]
        [Authorize(Roles ="Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id,  CancellationToken cancellationToken)=>FromResult(await _categoryService.DeleteCategoryAsync(id, cancellationToken));
    }
}
