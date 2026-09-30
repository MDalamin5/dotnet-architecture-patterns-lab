using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Models
{
    public class Category : ITenantEntity
{
    public Guid CategoryId { get; set; }
    public Guid TenantId { get; set; } // 👈 Added
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
}