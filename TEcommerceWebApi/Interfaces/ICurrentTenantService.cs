using System;

namespace TEcommerceWebApi.Interfaces
{
    public interface ICurrentTenantService
    {
        Guid? TenantId { get; }
        string? Subdomain { get; }
        void SetTenant(Guid tenantId, string? subdomain = null);
    }
}