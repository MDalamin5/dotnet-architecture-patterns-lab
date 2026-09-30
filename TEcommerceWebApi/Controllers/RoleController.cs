using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Controllers
{
    [ApiController]
    [Route("/api/v2/roles")]
    [Authorize(Roles = "Admin")] // 🔒 Only Admins can manage roles and permissions!
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // 1. Get list of all permissions (for rendering UI checkboxes)
        [HttpGet("permissions")]
        public async Task<ActionResult<ApiResponse<List<PermissionReadDto>>>> GetAllPermissions()
        {
            var permissions = await _roleService.GetAllPermissionsAsync();
            return Ok(ApiResponse<List<PermissionReadDto>>.SuccessResponse(permissions, 200, "Permissions retrieved."));
        }

        // 2. Get all roles with their assigned permissions
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RoleReadDto>>>> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(ApiResponse<List<RoleReadDto>>.SuccessResponse(roles, 200, "Roles retrieved."));
        }

        // 3. Get single role by ID
        [HttpGet("{roleId:guid}")]
        public async Task<ActionResult<ApiResponse<RoleReadDto>>> GetRoleById(Guid roleId)
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            if (role == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { "Role not found." }, 404, "Not Found"));
            }

            return Ok(ApiResponse<RoleReadDto>.SuccessResponse(role, 200, "Role retrieved."));
        }

        // 4. Create custom role with permissions
        [HttpPost]
        public async Task<ActionResult<ApiResponse<RoleReadDto>>> CreateRole([FromBody] RoleCreateDto roleData)
        {
            try
            {
                var createdRole = await _roleService.CreateRoleAsync(roleData);
                return CreatedAtAction(nameof(GetRoleById), new { roleId = createdRole.RoleId },
                    ApiResponse<RoleReadDto>.SuccessResponse(createdRole, 201, "Role created successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, 409, "Role Conflict"));
            }
        }

        // 5. Update toggle permissions for an existing role
        [HttpPut("{roleId:guid}/permissions")]
        public async Task<ActionResult<ApiResponse<RoleReadDto>>> UpdateRolePermissions(
            Guid roleId, 
            [FromBody] UpdateRolePermissionsDto updateDto)
        {
            var updatedRole = await _roleService.UpdateRolePermissionsAsync(roleId, updateDto.PermissionIds);
            if (updatedRole == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { "Role not found." }, 404, "Not Found"));
            }

            return Ok(ApiResponse<RoleReadDto>.SuccessResponse(updatedRole, 200, "Role permissions updated successfully."));
        }

        // 6. Assign role to a specific user
        [HttpPut("users/{userId:guid}/assign-role")]
        public async Task<ActionResult<ApiResponse<object>>> AssignRoleToUser(
            Guid userId, 
            [FromBody] AssignUserRoleDto dto)
        {
            try
            {
                var success = await _roleService.AssignRoleToUserAsync(userId, dto.RoleId);
                if (!success)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { "User not found." }, 404, "Not Found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, 200, "Role assigned to user successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, 404, "Role Not Found"));
            }
        }
    }
}