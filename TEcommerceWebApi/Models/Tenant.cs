using System;
using System.Collections.Generic;

namespace TEcommerceWebApi.Models
{
    public class Tenant
    {
        public Guid TenantId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty; // e.g. "nike"
        public string? CustomDomain { get; set; } // e.g. "www.nikestore.com"

        // 👑 The email of the merchant who owns this store:
        public string OwnerEmail { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Collections
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}