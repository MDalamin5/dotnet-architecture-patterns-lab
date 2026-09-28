using System;
using Microsoft.EntityFrameworkCore;
using AppPermissions = TEcommerceWebApi.Helpers.Permissions; // ✅ Alias to prevent name collision!
using TEcommerceWebApi.Models;

namespace TEcommerceWebApi.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // PBAC Tables
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // Global Query Filters (Soft Deletes)
            // ==========================================
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);

            // ==========================================
            // Category & Product Configuration
            // ==========================================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.ProductId);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Price).HasPrecision(18, 2);

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // Role, Permission & RolePermission (PBAC)
            // ==========================================
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleId);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(p => p.PermissionId);
                entity.Property(p => p.Code).IsRequired().HasMaxLength(100);
                entity.HasIndex(p => p.Code).IsUnique();
            });

            // Composite Primary Key for Junction Table
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

                entity.HasOne(rp => rp.Role)
                      .WithMany(r => r.RolePermissions)
                      .HasForeignKey(rp => rp.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(rp => rp.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // User Configuration
            // ==========================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();

                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // Order & OrderItems
            // ==========================================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
                entity.Property(o => o.Status).HasConversion<string>();

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.OrderItemId);
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);

                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 🌱 SEED DATA: Fixed GUIDs for Roles & Permissions
            // ==========================================
            var adminRoleId = Guid.Parse("10000000-0000-0000-0000-000000000001");
            var customerRoleId = Guid.Parse("20000000-0000-0000-0000-000000000002");

            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = adminRoleId, Name = "Admin", Description = "Full system access" },
                new Role { RoleId = customerRoleId, Name = "Customer", Description = "Regular store customer" }
            );

            // Seed Permissions
            var p1 = Guid.Parse("30000000-0000-0000-0000-000000000001");
            var p2 = Guid.Parse("30000000-0000-0000-0000-000000000002");
            var p3 = Guid.Parse("30000000-0000-0000-0000-000000000003");
            var p4 = Guid.Parse("30000000-0000-0000-0000-000000000004");
            var p5 = Guid.Parse("30000000-0000-0000-0000-000000000005");
            var p6 = Guid.Parse("30000000-0000-0000-0000-000000000006");
            var p7 = Guid.Parse("30000000-0000-0000-0000-000000000007");

            modelBuilder.Entity<Permission>().HasData(
                new Permission { PermissionId = p1, Code = AppPermissions.CategoriesCreate, Description = "Create categories" },
                new Permission { PermissionId = p2, Code = AppPermissions.CategoriesDelete, Description = "Delete categories" },
                new Permission { PermissionId = p3, Code = AppPermissions.ProductsCreate, Description = "Create products" },
                new Permission { PermissionId = p4, Code = AppPermissions.ProductsDelete, Description = "Delete products" },
                new Permission { PermissionId = p5, Code = AppPermissions.AnalyticsView, Description = "View analytics dashboards" },
                new Permission { PermissionId = p6, Code = AppPermissions.OrdersCreate, Description = "Checkout and create orders" },
                new Permission { PermissionId = p7, Code = AppPermissions.OrdersManageStatus, Description = "Update order statuses" }
            );

            // Assign All Permissions to Admin
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = adminRoleId, PermissionId = p1 },
                new RolePermission { RoleId = adminRoleId, PermissionId = p2 },
                new RolePermission { RoleId = adminRoleId, PermissionId = p3 },
                new RolePermission { RoleId = adminRoleId, PermissionId = p4 },
                new RolePermission { RoleId = adminRoleId, PermissionId = p5 },
                new RolePermission { RoleId = adminRoleId, PermissionId = p6 },
                new RolePermission { RoleId = adminRoleId, PermissionId = p7 },
                
                // Customer only gets 'orders.create'
                new RolePermission { RoleId = customerRoleId, PermissionId = p6 }
            );
        }
    }
}