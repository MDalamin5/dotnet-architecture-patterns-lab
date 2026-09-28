using System;
using System.Collections.Generic;

namespace TEcommerceWebApi.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // 🛡️ Foreign Key to Role Table
        public Guid RoleId { get; set; }
        public Role? Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}