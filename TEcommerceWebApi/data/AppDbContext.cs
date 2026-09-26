using Microsoft.EntityFrameworkCore;
using TEcommerceWebApi.Models;
using TEcommerceWebApi.Enums;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🛡️ Global Query Filters (Soft Deletes)
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);

            // ==========================================
            // 1. Category & Product Configuration
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
            // 2. User Configuration
            // ==========================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                
                // Store Enum as string (e.g. "Admin", "Customer")
                entity.Property(u => u.Role).HasConversion<string>();

                entity.HasIndex(u => u.Email).IsUnique();
            });

            // ==========================================
            // 3. Order Configuration
            // ==========================================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

                // Convert Enum to String or Int in DB (Store as string or int)
                entity.Property(o => o.Status).HasConversion<string>();

                // User 1:M Order
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // Don't delete user if orders exist
            });

            // ==========================================
            // 4. OrderItem Configuration (The Bridge)
            // ==========================================
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.OrderItemId);
                entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2);

                // Order 1:M OrderItem (Cascade: If an Order is deleted, delete its line items)
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Product 1:M OrderItem (Restrict: Never delete a Product if it was ordered in the past!)
                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}