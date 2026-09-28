using System;
using System.Collections.Generic;

namespace TEcommerceWebApi.Models
{
    public class Role
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; } = string.Empty; // e.g. "Admin", "Customer", "Manager"
        public string Description { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}