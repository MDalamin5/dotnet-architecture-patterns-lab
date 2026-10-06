using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TEcommerceWebApi.data;
using TEcommerceWebApi.Interfaces;

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
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

            // Skip resolution on public platform routes
            if (path.StartsWith("/swagger") || path == "/api/v2/tenants" || path == "/")
            {
                await _next(context);
                return;
            }

            Guid? resolvedTenantId = null;

            // Strategy 1: Header 'X-Tenant-Id'
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) && 
                Guid.TryParse(tenantHeader, out var headerGuid))
            {
                resolvedTenantId = headerGuid;
            }
            // Strategy 2: JWT Claim 'TenantId' (for authenticated requests)
            else if (context.User.Identity?.IsAuthenticated == true)
            {
                var claimTenantId = context.User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
                if (Guid.TryParse(claimTenantId, out var claimGuid))
                {
                    resolvedTenantId = claimGuid;
                }
            }
            // Strategy 3: Subdomain or Custom Domain from Host header
            else
            {
                var host = context.Request.Host.Host.ToLower();

                // Only search DB if host is not plain localhost or an IP
                if (host != "localhost" && !host.StartsWith("127.0.0.1"))
                {
                    using var scope = serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var tenant = await dbContext.Tenants
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.CustomDomain == host || t.Subdomain == host.Split('.')[0]);

                    if (tenant != null)
                    {
                        resolvedTenantId = tenant.TenantId;
                    }
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