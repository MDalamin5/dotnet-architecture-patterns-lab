using System;

namespace TEcommerceWebApi.DTOs
{
    public class ProductReadDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; } // 👈 Added
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}