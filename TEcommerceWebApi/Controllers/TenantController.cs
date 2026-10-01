using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEcommerceWebApi.data;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Models;

namespace TEcommerceWebApi.Controllers
{
    [ApiController]
    [Route("/api/v2/tenants")]
    public class TenantController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public TenantController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // 1. Create a New Store (Merchant Onboarding)
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TenantReadDto>>> CreateTenant([FromBody] TenantCreateDto dto)
        {
            var subdomainLower = dto.Subdomain.Trim().ToLower();

            // Check if subdomain is taken
            var exists = await _appDbContext.Tenants.AnyAsync(t => t.Subdomain == subdomainLower);
            if (exists)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Subdomain '{dto.Subdomain}' is already taken." }, 409, "Subdomain Unavailable"));
            }

            var newTenantId = Guid.NewGuid();
            var ownerEmailLower = dto.OwnerEmail.Trim().ToLower();

            // 1. Create the Tenant entity
            var tenant = new Tenant
            {
                TenantId = newTenantId,
                StoreName = dto.StoreName.Trim(),
                Subdomain = subdomainLower,
                CustomDomain = dto.CustomDomain?.Trim().ToLower(),
                OwnerEmail = ownerEmailLower, // 👑 Assigned to this merchant!
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // 2. Fetch Admin Role
            var adminRole = await _appDbContext.Roles.FirstAsync(r => r.Name == "Admin");

            // 3. Automatically create the Admin User for this new store!
            var adminUser = new User
            {
                UserId = Guid.NewGuid(),
                TenantId = newTenantId, // Linked to this new store
                Email = ownerEmailLower,
                FullName = dto.OwnerFullName.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.OwnerPassword),
                RoleId = adminRole.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            await _appDbContext.Tenants.AddAsync(tenant);
            await _appDbContext.Users.AddAsync(adminUser);
            await _appDbContext.SaveChangesAsync();

            var responseDto = new TenantReadDto
            {
                TenantId = tenant.TenantId,
                StoreName = tenant.StoreName,
                Subdomain = tenant.Subdomain,
                CustomDomain = tenant.CustomDomain,
                OwnerEmail = tenant.OwnerEmail,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };

            return StatusCode(201, ApiResponse<TenantReadDto>.SuccessResponse(
                responseDto, 201, $"Store '{tenant.StoreName}' created successfully at {tenant.Subdomain}.byvbd.com!"));
        }

        // 2. Merchant Store Switcher: Get all stores owned by the currently logged-in Merchant
        [HttpGet("my-stores")]
        [Authorize] // 🔒 Any authenticated user
        public async Task<ActionResult<ApiResponse<List<TenantReadDto>>>> GetMyStores()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(new List<string> { "User email claim missing." }, 401, "Unauthorized"));
            }

            // Query the Tenants table for stores matching this owner's email
            var stores = await _appDbContext.Tenants
                .AsNoTracking()
                .Where(t => t.OwnerEmail.ToLower() == userEmail.ToLower())
                .Select(t => new TenantReadDto
                {
                    TenantId = t.TenantId,
                    StoreName = t.StoreName,
                    Subdomain = t.Subdomain,
                    CustomDomain = t.CustomDomain,
                    OwnerEmail = t.OwnerEmail,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<TenantReadDto>>.SuccessResponse(stores, 200, "Merchant stores retrieved successfully."));
        }
    }
}