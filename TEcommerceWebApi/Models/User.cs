using System;
using System.Collections.Generic;
using TEcommerceWebApi.Enums;

namespace TEcommerceWebApi.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        // 🔒 Store the one-way hashed password (never plain text!)
        public string PasswordHash { get; set; } = string.Empty;

        // 🛡️ The User's Role for RBAC
        public UserRole Role { get; set; } = UserRole.Customer;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}