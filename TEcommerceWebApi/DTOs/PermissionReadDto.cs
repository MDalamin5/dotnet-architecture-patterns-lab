using System;

namespace TEcommerceWebApi.DTOs
{
    public class PermissionReadDto
    {
        public Guid PermissionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}