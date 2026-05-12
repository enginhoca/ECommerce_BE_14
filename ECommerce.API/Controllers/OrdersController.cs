using System.Security.Claims;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Produces("application/json")]
    public class OrdersController : ApiControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>),StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto,  CancellationToken cancellationToken)
        {
            var customerId=User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _orderService.CreateOrderAsync(dto, customerId, cancellationToken);
            if(!result.IsSuccess) return FromResult(result);
            return CreatedAtAction(nameof(GetById),new {id=result.Value!.Id}, result.Value);
        }

        [HttpGet("{id:int}",Name =nameof(GetById))]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            if(!result.IsSuccess) return FromResult(result);
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!User.IsInRole("Admin") && !User.IsInRole("Manager") && result.Value!.CustomerId!=customerId)
            {
                return FromResult(Result<OrderDto>.Forbidden("Bu siparişe erişim yetkiniz yok!"));
            }
            return FromResult(result);
        }

        [HttpGet("my")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return FromResult(await _orderService.GetCustomerOrdersAsync(customerId, cancellationToken));
        }


        [HttpGet]
        [Authorize(Roles ="Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponseDto<OrderDto>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDto pagination, CancellationToken cancellationToken) => FromResult(await _orderService.GetAllOrdersAsync(pagination, cancellationToken));

        [HttpPut("{id:int}/status")]
        [Authorize(Roles="Admin, Manager")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDto dto, CancellationToken cancellationToken) => FromResult(await _orderService.UpdateOrderStatusAsync(id, dto.Status, cancellationToken));
    }
}
