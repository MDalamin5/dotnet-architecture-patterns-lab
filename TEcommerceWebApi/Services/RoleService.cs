using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TEcommerceWebApi.data;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Interfaces;
using TEcommerceWebApi.Models;

namespace TEcommerceWebApi.Services
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _appDbContext;

        public RoleService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // =========================================================================
        // 1. GET ALL PERMISSIONS (Used to render UI checkboxes)
        // =========================================================================
        public async Task<List<PermissionReadDto>> GetAllPermissionsAsync()
        {
            return await _appDbContext.Permissions
                .AsNoTracking()
                .OrderBy(p => p.Code)
                .Select(p => new PermissionReadDto
                {
                    PermissionId = p.PermissionId,
                    Code = p.Code,
                    Description = p.Description
                })
                .ToListAsync();
        }

        // =========================================================================
        // 2. GET ALL ROLES (With their active permissions)
        // =========================================================================
        public async Task<List<RoleReadDto>> GetAllRolesAsync()
        {
            return await _appDbContext.Roles
                .AsNoTracking()
                .Select(r => new RoleReadDto
                {
                    RoleId = r.RoleId,
                    Name = r.Name,
                    Description = r.Description,
                    Permissions = r.RolePermissions.Select(rp => new PermissionReadDto
                    {
                        PermissionId = rp.Permission.PermissionId,
                        Code = rp.Permission.Code,
                        Description = rp.Permission.Description
                    }).ToList()
                })
                .ToListAsync();
        }

        // =========================================================================
        // 3. GET SINGLE ROLE BY ID
        // =========================================================================
        public async Task<RoleReadDto?> GetRoleByIdAsync(Guid roleId)
        {
            return await _appDbContext.Roles
                .AsNoTracking()
                .Where(r => r.RoleId == roleId)
                .Select(r => new RoleReadDto
                {
                    RoleId = r.RoleId,
                    Name = r.Name,
                    Description = r.Description,
                    Permissions = r.RolePermissions.Select(rp => new PermissionReadDto
                    {
                        PermissionId = rp.Permission.PermissionId,
                        Code = rp.Permission.Code,
                        Description = rp.Permission.Description
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        // =========================================================================
        // 4. CREATE DYNAMIC ROLE (With Junction Table Inserts)
        // =========================================================================
        public async Task<RoleReadDto> CreateRoleAsync(RoleCreateDto roleData)
        {
            // Step A: Check if role name already exists
            var roleExists = await _appDbContext.Roles
                .AnyAsync(r => r.Name.ToLower() == roleData.Name.Trim().ToLower());

            if (roleExists)
            {
                throw new InvalidOperationException($"Role with name '{roleData.Name}' already exists.");
            }

            var newRoleId = Guid.NewGuid();

            // Step B: Verify the submitted Permission IDs exist in the database
            var validPermissionIds = await _appDbContext.Permissions
                .Where(p => roleData.PermissionIds.Contains(p.PermissionId))
                .Select(p => p.PermissionId)
                .ToListAsync();

            // Step C: Build Role entity
            var role = new Role
            {
                RoleId = newRoleId,
                Name = roleData.Name.Trim(),
                Description = roleData.Description.Trim(),
                // Build Junction Table records in memory:
                RolePermissions = validPermissionIds.Select(pId => new RolePermission
                {
                    RoleId = newRoleId,
                    PermissionId = pId
                }).ToList()
            };

            await _appDbContext.Roles.AddAsync(role);
            await _appDbContext.SaveChangesAsync();

            return await GetRoleByIdAsync(newRoleId) 
                ?? throw new Exception("Error loading created role.");
        }

        // =========================================================================
        // 5. UPDATE ROLE PERMISSIONS (Checkbox Toggle Synchronization)
        // =========================================================================
        public async Task<RoleReadDto?> UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds)
        {
            // Step A: Fetch role with its existing permissions
            var role = await _appDbContext.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null) return null;

            // Step B: Clear old junction records
            role.RolePermissions.Clear();

            // Step C: Verify new permission IDs exist
            var validPermissionIds = await _appDbContext.Permissions
                .Where(p => permissionIds.Contains(p.PermissionId))
                .Select(p => p.PermissionId)
                .ToListAsync();

            // Step D: Insert the new selected permissions
            foreach (var pId in validPermissionIds)
            {
                role.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.RoleId,
                    PermissionId = pId
                });
            }

            // EF Core runs: DELETE old junction rows + INSERT new junction rows
            await _appDbContext.SaveChangesAsync();

            return await GetRoleByIdAsync(roleId);
        }

        // =========================================================================
        // 6. ASSIGN ROLE TO USER
        // =========================================================================
        public async Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            var user = await _appDbContext.Users.FindAsync(userId);
            if (user == null) return false;

            var roleExists = await _appDbContext.Roles.AnyAsync(r => r.RoleId == roleId);
            if (!roleExists)
            {
                throw new KeyNotFoundException($"Role with ID '{roleId}' does not exist.");
            }

            // Update user's foreign key
            user.RoleId = roleId;
            await _appDbContext.SaveChangesAsync();

            return true;
        }
    }
}