using System;
using System.Collections.Generic;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Models
{
    public class User : ITenantEntity
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; } // 👈 Added
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public Role? Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}