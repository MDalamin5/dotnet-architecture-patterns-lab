using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TEcommerceWebApi.DTOs;

namespace TEcommerceWebApi.Interfaces
{
    public interface IRoleService
    {
        Task<List<PermissionReadDto>> GetAllPermissionsAsync();
        Task<List<RoleReadDto>> GetAllRolesAsync();
        Task<RoleReadDto?> GetRoleByIdAsync(Guid roleId);
        Task<RoleReadDto> CreateRoleAsync(RoleCreateDto roleData);
        Task<RoleReadDto?> UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds);
        Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId);
    }
}