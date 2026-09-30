using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        // Create a New Store (Tenant Onboarding)
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TenantReadDto>>> CreateTenant([FromBody] TenantCreateDto dto)
        {
            var subdomainLower = dto.Subdomain.Trim().ToLower();

            // Check if subdomain is already claimed
            var exists = await _appDbContext.Tenants.AnyAsync(t => t.Subdomain == subdomainLower);
            if (exists)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Subdomain '{dto.Subdomain}' is already taken." }, 409, "Subdomain Unavailable"));
            }

            var tenant = new Tenant
            {
                TenantId = Guid.NewGuid(),
                StoreName = dto.StoreName.Trim(),
                Subdomain = subdomainLower,
                CustomDomain = dto.CustomDomain?.Trim().ToLower(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _appDbContext.Tenants.AddAsync(tenant);
            await _appDbContext.SaveChangesAsync();

            var responseDto = new TenantReadDto
            {
                TenantId = tenant.TenantId,
                StoreName = tenant.StoreName,
                Subdomain = tenant.Subdomain,
                CustomDomain = tenant.CustomDomain,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt
            };

            return StatusCode(201, ApiResponse<TenantReadDto>.SuccessResponse(responseDto, 201, "Store created successfully."));
        }
    }
}