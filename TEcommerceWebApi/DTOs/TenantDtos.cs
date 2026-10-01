using System;
using System.ComponentModel.DataAnnotations;

namespace TEcommerceWebApi.DTOs
{
    // Payload to create a new store
    public class TenantCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string StoreName { get; set; } = string.Empty; // e.g. "Nike Official"

        [Required]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Subdomain can only contain lowercase letters, numbers, and hyphens.")]
        [StringLength(50, MinimumLength = 3)]
        public string Subdomain { get; set; } = string.Empty; // e.g. "nike"

        public string? CustomDomain { get; set; } // e.g. "www.nikestore.com"

        // Owner/Admin Credentials for this new store
        [Required]
        [EmailAddress]
        public string OwnerEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string OwnerFullName { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string OwnerPassword { get; set; } = string.Empty;
    }

    public class TenantReadDto
    {
        public Guid TenantId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string? CustomDomain { get; set; }
        public string OwnerEmail { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}