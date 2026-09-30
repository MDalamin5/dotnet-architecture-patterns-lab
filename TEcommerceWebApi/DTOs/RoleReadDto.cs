using System;
using System.Collections.Generic;

namespace TEcommerceWebApi.DTOs
{
    public class RoleReadDto
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // The list of permissions this role currently possesses
        public List<PermissionReadDto> Permissions { get; set; } = new List<PermissionReadDto>();
    }
}