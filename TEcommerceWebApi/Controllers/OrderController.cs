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

        // 1. Checkout (Any Logged-in Customer/Admin)
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, 401, "Authentication Required"));
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

        // 2. Customer views their own order history
        [HttpGet("my-orders")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<OrderReadDto>>>> GetMyOrders([FromQuery] OrderStatus? status)
        {
            try
            {
                var orders = await _orderService.GetMyOrdersAsync(status);
                return Ok(ApiResponse<List<OrderReadDto>>.SuccessResponse(orders, 200, "Your order history retrieved."));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, 401, "Authentication Required"));
            }
        }

        // 3. Get single order invoice by Order ID
        [HttpGet("{orderId:guid}")]
        [Authorize]
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

        // 4. Admin views ANY specific user's orders
        [HttpGet("user/{userId:guid}")]
        [Authorize(Roles = "Admin")]
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

        // 5. Admin lists all orders with pagination & status filter
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PaginatedResult<OrderReadDto>>>> GetAllOrders(
            [FromQuery] QueryParameters queryParameters,
            [FromQuery] OrderStatus? status)
        {
            queryParameters.Validate();
            var result = await _orderService.GetAllOrdersAsync(queryParameters, status);
            return Ok(ApiResponse<PaginatedResult<OrderReadDto>>.SuccessResponse(result, 200, "All orders retrieved."));
        }

        // 6. Admin updates order status (e.g. Processing -> Completed)
        [HttpPatch("{orderId:guid}/status")]
        [Authorize(Roles = "Admin")]
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