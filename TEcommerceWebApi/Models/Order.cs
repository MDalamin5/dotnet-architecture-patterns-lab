using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TEcommerceWebApi.Enums;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Models
{
    public class Order : ITenantEntity
    {
        public Guid OrderId { get; set; }
        public Guid TenantId { get; set; } // 👈 Added
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}