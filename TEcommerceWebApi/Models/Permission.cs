using System;
using System.Collections.Generic;

namespace TEcommerceWebApi.Models
{
    public class Permission
    {
        public Guid PermissionId { get; set; }
        public string Code { get; set; } = string.Empty; // e.g. "categories.delete"
        public string Description { get; set; } = string.Empty;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}