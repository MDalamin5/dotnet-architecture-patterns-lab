using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TEcommerceWebApi.Enums;

namespace TEcommerceWebApi.DTOs
{
    public class OrderCheckoutDto
    {
        // ❌ Removed 'public Guid UserId { get; set; }' (Extracted automatically from Token!)

        [Required]
        [MinLength(1, ErrorMessage = "An order must have at least 1 item.")]
        public List<OrderItemCheckoutDto> Items { get; set; } = new List<OrderItemCheckoutDto>();
    }
    

    public class OrderItemCheckoutDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
        public int Quantity { get; set; }
    }

    public class OrderStatusUpdateDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}

/*
🔒 Security Notice: Notice that OrderItemCheckoutDto only takes ProductId and Quantity. Never let the client send the Price! The backend must fetch the verified price from PostgreSQL.
*/