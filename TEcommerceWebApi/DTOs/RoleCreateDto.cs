using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TEcommerceWebApi.DTOs
{
    public class RoleCreateDto
    {
        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters.")]
        public string Name { get; set; } = string.Empty; // e.g. "ContentModerator"

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        // List of Permission IDs selected from the checkboxes in the UI
        public List<Guid> PermissionIds { get; set; } = new List<Guid>();
    }
}