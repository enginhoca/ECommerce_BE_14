using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Produces("application/json")]
    public class ProductsController : ApiControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }


        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponseDto<ProductDto>>),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter, [FromQuery] PaginationDto pagination, CancellationToken cancellationToken)=>FromResult(await _productService.GetProductsAsync(filter, pagination, cancellationToken));


        [HttpGet("{id:int}", Name =nameof(GetProductById))]
        [ActionName(nameof(GetProductById))]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(int id, CancellationToken cancellationToken)=>FromResult(await _productService.GetProductByIdAsync(id, cancellationToken));


        [HttpPost]
        [Authorize(Roles="Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>),StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto,CancellationToken cancellationToken)
        {
            var result = await _productService.CreateProductAsync(dto, cancellationToken);
            if(!result.IsSuccess) return FromResult(result);
            return CreatedAtAction(nameof(GetProductById), new { id= result.Value!.Id}, result.Value);
        }


        [HttpPut("{id:int}")]
        [Authorize(Roles="Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>),StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto, CancellationToken cancellationToken)=>FromResult(await _productService.UpdateProductAsync(id, dto, cancellationToken));


        [HttpDelete("{id:int}")]
        [Authorize(Roles="Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)=> FromResult(await _productService.DeleteProductAsync(id, cancellationToken));



        [HttpPost("{id:int}/image")]
        [Authorize(Roles="Admin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<string>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadImage(int id, IFormFile file, CancellationToken cancellationToken)=>FromResult(await _productService.UploadImageAsync(id, file.OpenReadStream(), file.FileName, file.ContentType, cancellationToken));
    }
}
