using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TEcommerceWebApi.data;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Enums;
using TEcommerceWebApi.Helpers;
using TEcommerceWebApi.Interfaces;
using TEcommerceWebApi.Models;
using TEcommerceWebApi.Controllers;

namespace TEcommerceWebApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ICurrentUserService _currentUserService;

        public OrderService(AppDbContext appDbContext, ICurrentUserService currentUserService)
        {
            _appDbContext = appDbContext;
            _currentUserService = currentUserService;
        }

        // 1. Checkout (Transactional, Stock Deducting, Secure Current User)
        public async Task<OrderReadDto> CheckoutAsync(OrderCheckoutDto checkoutData)
        {
            // Read UserId directly from JWT token
            var currentUserId = _currentUserService.UserId;

            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var user = await _appDbContext.Users.FindAsync(currentUserId.Value);
            if (user == null)
            {
                throw new KeyNotFoundException("Authenticated user was not found in the database.");
            }

            // Batch fetch all requested products in one SQL query
            var productIds = checkoutData.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _appDbContext.Products
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            // Begin Atomic Database Transaction
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                var orderItems = new List<OrderItem>();
                decimal totalAmount = 0;
                var newOrderId = Guid.NewGuid();

                foreach (var itemDto in checkoutData.Items)
                {
                    var product = products.FirstOrDefault(p => p.ProductId == itemDto.ProductId);
                    if (product == null)
                    {
                        throw new KeyNotFoundException($"Product with ID '{itemDto.ProductId}' was not found.");
                    }

                    // Check stock availability
                    if (product.StockQuantity < itemDto.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}, Requested: {itemDto.Quantity}."
                        );
                    }

                    // Deduct stock
                    product.StockQuantity -= itemDto.Quantity;

                    // Calculate item total based on current database price
                    var itemTotal = product.Price * itemDto.Quantity;
                    totalAmount += itemTotal;

                    // Create line item record
                    orderItems.Add(new OrderItem
                    {
                        OrderItemId = Guid.NewGuid(),
                        OrderId = newOrderId,
                        ProductId = product.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price // Snapshot of current price
                    });
                }

                // Create parent Order entity
                var order = new Order
                {
                    OrderId = newOrderId,
                    UserId = user.UserId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    OrderItems = orderItems
                };

                await _appDbContext.Orders.AddAsync(order);
                await _appDbContext.SaveChangesAsync();

                // Commit transaction to database
                await transaction.CommitAsync();

                return await GetOrderByIdAsync(newOrderId) 
                    ?? throw new Exception("Error loading created order.");
            }
            catch
            {
                // Rollback if any step fails
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 2. Get Single Order by Order ID
        public async Task<OrderReadDto?> GetOrderByIdAsync(Guid orderId)
        {
            return await _appDbContext.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == orderId)
                .Select(o => new OrderReadDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    CustomerName = o.User != null ? o.User.FullName : string.Empty,
                    CustomerEmail = o.User != null ? o.User.Email : string.Empty,
                    Items = o.OrderItems.Select(oi => new OrderItemReadDto
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.Name : string.Empty,
                        CategoryName = oi.Product != null && oi.Product.Category != null 
                                       ? oi.Product.Category.Name 
                                       : string.Empty,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        // 3. Customer views their own orders (Derived from JWT claims)
        public async Task<List<OrderReadDto>> GetMyOrdersAsync(OrderStatus? status = null)
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return await GetOrdersByUserIdAsync(currentUserId.Value, status) ?? new List<OrderReadDto>();
        }

        // 4. Get Orders for any specific user ID
        public async Task<List<OrderReadDto>?> GetOrdersByUserIdAsync(Guid userId, OrderStatus? status = null)
        {
            var userExists = await _appDbContext.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists) return null;

            var query = _appDbContext.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderReadDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    CustomerName = o.User != null ? o.User.FullName : string.Empty,
                    CustomerEmail = o.User != null ? o.User.Email : string.Empty,
                    Items = o.OrderItems.Select(oi => new OrderItemReadDto
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.Name : string.Empty,
                        CategoryName = oi.Product != null && oi.Product.Category != null 
                                       ? oi.Product.Category.Name 
                                       : string.Empty,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync();
        }

        // 5. Admin Paginated Orders List
        public async Task<PaginatedResult<OrderReadDto>> GetAllOrdersAsync(
            QueryParameters queryParameters, 
            OrderStatus? status = null)
        {
            var query = _appDbContext.Orders
                .AsNoTracking()
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            query = query.OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize)
                .Select(o => new OrderReadDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    CustomerName = o.User != null ? o.User.FullName : string.Empty,
                    CustomerEmail = o.User != null ? o.User.Email : string.Empty,
                    Items = o.OrderItems.Select(oi => new OrderItemReadDto
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.Name : string.Empty,
                        CategoryName = oi.Product != null && oi.Product.Category != null 
                                       ? oi.Product.Category.Name 
                                       : string.Empty,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return new PaginatedResult<OrderReadDto>
            {
                Items = orders,
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize
            };
        }

        // 6. Update Order Status
        public async Task<OrderReadDto?> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _appDbContext.Orders.FindAsync(orderId);
            if (order == null) return null;

            order.Status = newStatus;
            await _appDbContext.SaveChangesAsync();

            return await GetOrderByIdAsync(orderId);
        }
    }
}