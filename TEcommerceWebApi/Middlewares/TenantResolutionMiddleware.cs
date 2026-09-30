using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TEcommerceWebApi.data;
using TEcommerceWebApi.Interfaces;
using TEcommerceWebApi.Models;

namespace TEcommerceWebApi.Middlewares
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentTenantService currentTenantService, IServiceProvider serviceProvider)
        {
            // Path exclusion: Skip tenant resolution for tenant creation endpoint, swagger, or root health checks
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
            if (path.StartsWith("/swagger") || path == "/api/v2/tenants" || path == "/")
            {
                await _next(context);
                return;
            }

            Guid? resolvedTenantId = null;

            // Strategy 1: Check HTTP Header 'X-Tenant-Id' (Great for Swagger & Mobile apps)
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) && 
                Guid.TryParse(tenantHeader, out var headerGuid))
            {
                resolvedTenantId = headerGuid;
            }
            // Strategy 2: Check JWT Claim 'TenantId' (For logged-in users)
            else if (context.User.Identity?.IsAuthenticated == true)
            {
                var claimTenantId = context.User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
                if (Guid.TryParse(claimTenantId, out var claimGuid))
                {
                    resolvedTenantId = claimGuid;
                }
            }
            // Strategy 3: Check Host Header / Subdomain (e.g. nike.byvstore.com or www.nikestore.com)
            else
            {
                var host = context.Request.Host.Host.ToLower();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Check for custom domain match or subdomain match
                var tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.CustomDomain == host || t.Subdomain == host.Split('.')[0]);

                if (tenant != null)
                {
                    resolvedTenantId = tenant.TenantId;
                }
            }

            if (resolvedTenantId.HasValue)
            {
                currentTenantService.SetTenant(resolvedTenantId.Value);
            }

            await _next(context);
        }
    }
}