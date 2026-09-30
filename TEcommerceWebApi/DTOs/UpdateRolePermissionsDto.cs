using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TEcommerceWebApi.DTOs
{
    public class UpdateRolePermissionsDto
    {
        [Required]
        public List<Guid> PermissionIds { get; set; } = new List<Guid>();
    }

    public class AssignUserRoleDto
    {
        [Required]
        public Guid RoleId { get; set; }
    }
}