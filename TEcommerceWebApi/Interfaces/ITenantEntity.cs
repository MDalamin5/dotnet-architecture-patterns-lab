using System;

namespace TEcommerceWebApi.Interfaces
{
    public interface ITenantEntity
    {
        Guid TenantId { get; set; }
    }
}