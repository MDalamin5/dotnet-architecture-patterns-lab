using System;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        public Guid? TenantId { get; private set; }
        public string? Subdomain { get; private set; }

        public void SetTenant(Guid tenantId, string? subdomain = null)
        {
            TenantId = tenantId;
            Subdomain = subdomain;
        }
    }
}