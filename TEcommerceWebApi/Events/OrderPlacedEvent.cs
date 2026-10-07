using System;

namespace TEcommerceWebApi.Events
{
    public class OrderPlacedEvent
    {
        public Guid OrderId { get; set; }
        public Guid TenantId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public DateTime OrderDate { get; set; }
    }
}