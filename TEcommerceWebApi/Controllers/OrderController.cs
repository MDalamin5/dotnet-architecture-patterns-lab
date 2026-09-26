using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Enums;
using TEcommerceWebApi.Helpers;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Controllers
{
    [ApiController]
    [Route("/api/v2/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // 1. Checkout (Create Order with atomic transaction)
        [HttpPost("checkout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<OrderReadDto>>> Checkout([FromBody] OrderCheckoutDto checkoutData)
        {
            try
            {
                var order = await _orderService.CheckoutAsync(checkoutData);
                return CreatedAtAction(
                    nameof(GetOrderById), 
                    new { orderId = order.OrderId }, 
                    ApiResponse<OrderReadDto>.SuccessResponse(order, 201, "Order placed successfully.")
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, 404, "Validation Failed"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, 400, "Insufficient Stock"));
            }
        }

        // 2. Get Single Order Invoice by ID
        [HttpGet("{orderId:guid}")]
        public async Task<ActionResult<ApiResponse<OrderReadDto>>> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Order with ID '{orderId}' was not found." }, 
                    404, 
                    "Order Not Found"
                ));
            }

            return Ok(ApiResponse<OrderReadDto>.SuccessResponse(order, 200, "Order retrieved successfully."));
        }

        // 3. Get User Orders
        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<List<OrderReadDto>>>> GetUserOrders(
            Guid userId, 
            [FromQuery] OrderStatus? status)
        {
            var orders = await _orderService.GetOrdersByUserIdAsync(userId, status);
            if (orders == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"User with ID '{userId}' does not exist." },
                    404,
                    "User Not Found"
                ));
            }

            return Ok(ApiResponse<List<OrderReadDto>>.SuccessResponse(orders, 200, "User orders retrieved."));
        }

        // 4. Admin Get All Orders (Paginated + Status filter)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<OrderReadDto>>>> GetAllOrders(
            [FromQuery] QueryParameters queryParameters,
            [FromQuery] OrderStatus? status)
        {
            queryParameters.Validate();
            var result = await _orderService.GetAllOrdersAsync(queryParameters, status);
            return Ok(ApiResponse<PaginatedResult<OrderReadDto>>.SuccessResponse(result, 200, "All orders retrieved."));
        }

        // 5. Update Order Status
        [HttpPatch("{orderId:guid}/status")]
        public async Task<ActionResult<ApiResponse<OrderReadDto>>> UpdateOrderStatus(
            Guid orderId, 
            [FromBody] OrderStatusUpdateDto statusDto)
        {
            var updatedOrder = await _orderService.UpdateOrderStatusAsync(orderId, statusDto.Status);
            if (updatedOrder == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Order with ID '{orderId}' was not found." },
                    404,
                    "Order Not Found"
                ));
            }

            return Ok(ApiResponse<OrderReadDto>.SuccessResponse(updatedOrder, 200, "Order status updated successfully."));
        }
    }
}