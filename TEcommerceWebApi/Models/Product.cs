using TEcommerceWebApi.Interfaces;
namespace TEcommerceWebApi.Models
{
    public class Product : ITenantEntity
    {
        public Guid ProductId { get; set; }
        public Guid TenantId { get; set; } // 👈 Added
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; } // 👈 Stores public URL to image in MinIO
        public int StockQuantity { get; set; }
        public bool IsDeleted { get; set; } = false;
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}