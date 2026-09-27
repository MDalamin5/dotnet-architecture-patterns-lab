using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Enums;
using TEcommerceWebApi.Helpers;
using TEcommerceWebApi.Controllers;

namespace TEcommerceWebApi.Interfaces
{
    public interface IOrderService
    {
        Task<OrderReadDto> CheckoutAsync(OrderCheckoutDto checkoutData);
        Task<OrderReadDto?> GetOrderByIdAsync(Guid orderId);
        Task<List<OrderReadDto>> GetMyOrdersAsync(OrderStatus? status = null);
        Task<List<OrderReadDto>?> GetOrdersByUserIdAsync(Guid userId, OrderStatus? status = null);
        Task<PaginatedResult<OrderReadDto>> GetAllOrdersAsync(QueryParameters queryParameters, OrderStatus? status = null);
        Task<OrderReadDto?> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
    }
}