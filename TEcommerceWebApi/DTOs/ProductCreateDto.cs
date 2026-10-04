using System;
using System.ComponentModel.DataAnnotations;

namespace TEcommerceWebApi.DTOs
{
    public class ProductCreateDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 100000.00)]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int StockQuantity { get; set; } = 0; // 👈 Added
        public IFormFile? Image { get; set; } // 👈 Accepts image upload file from multipart form

        [Required]
        public Guid CategoryId { get; set; }
    }
}