using AutoMapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TEcommerceWebApi.Controllers;
using TEcommerceWebApi.data;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Helpers;
using TEcommerceWebApi.Interfaces;
using TEcommerceWebApi.Models;
using TEcommerceWebApi.Enums;

namespace TEcommerceWebApi.Services
{
        public class ProductService : IProductService
        {
            private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService; // 👈 Inject
        private readonly ICurrentTenantService _currentTenantService; // 👈 Inject

        public ProductService(
            AppDbContext appDbContext, 
            IMapper mapper, 
            IFileStorageService fileStorageService,
            ICurrentTenantService currentTenantService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _currentTenantService = currentTenantService;
        }

        public async Task<ProductReadDto?> CreateProduct(ProductCreateDto productData)
        {
            var category = await _appDbContext.Categories.FindAsync(productData.CategoryId);
            if (category == null) return null;

            string? uploadedImageUrl = null;

            // Upload image to MinIO if provided
            if (productData.Image != null)
            {
                var tenantFolder = $"tenants/{_currentTenantService.TenantId ?? Guid.Empty}/products";
                uploadedImageUrl = await _fileStorageService.UploadImageAsync(productData.Image, tenantFolder);
            }

            var newProduct = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = productData.Name,
                Price = productData.Price,
                StockQuantity = productData.StockQuantity,
                CategoryId = productData.CategoryId,
                ImageUrl = uploadedImageUrl // 👈 Saved URL
            };

            await _appDbContext.Products.AddAsync(newProduct);
            await _appDbContext.SaveChangesAsync();

            var responseDto = _mapper.Map<ProductReadDto>(newProduct);
            responseDto.CategoryName = category.Name;
            return responseDto;
        }

        public async Task<PaginatedResult<ProductReadDto>> GetAllProducts(QueryParameters queryParameter)
        {
            IQueryable<Product>? query = _appDbContext.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

            // Searching Performing
            if (!string.IsNullOrWhiteSpace(queryParameter.SearchValue))
            {
               
                var formattedSearch = $"%{queryParameter.SearchValue.Trim()}%";

                query = query.Where(p => EF.Functions.ILike(p.Name, formattedSearch) || EF.Functions.ILike(p.Category.Name, formattedSearch));
            
            }
            // start to implement the product sorting
            if (!string.IsNullOrWhiteSpace(queryParameter.SortOrder))
            {
                var formattedSortOrder = queryParameter.SortOrder.Trim();

                // 1. Parse into enum variable 'parsedSortOrder'
                if (Enum.TryParse<SortOrder>(formattedSortOrder, true, out var parsedSortOrder))
                {
                    // 2. Switch on the parsed enum 👇
                    switch (parsedSortOrder)
                    {
                        case SortOrder.NameAsc:
                            query = query.OrderBy(p => p.Name);
                            break;

                        case SortOrder.NameDesc:
                            query = query.OrderByDescending(p => p.Name);
                            break;

                        default:
                            query = query.OrderBy(p => p.Name);
                            break;
                    }
                }
                else
                {
                    // If user sends invalid text like ?sortOrder=invalidText
                    query = query.OrderBy(p => p.Name);
                }
            }
            else
            {
                // Default sorting if queryParameter.SortOrder is null
                query = query.OrderBy(p => p.Name);
            }

            var totalCount = await query.CountAsync();

            var items = await query
            .Skip((queryParameter.PageNumber -1) * queryParameter.PageSize).Take(queryParameter.PageSize)
            .Select(p => new ProductReadDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty
            }).ToListAsync();

            return new PaginatedResult<ProductReadDto>{
                Items = items,
                TotalCount = totalCount,
                PageNumber = queryParameter.PageNumber,
                PageSize = queryParameter.PageSize
            };
            // Direct projection using Select: Generates optimal SQL INNER JOIN
            // return await _appDbContext.Products
            //     .AsNoTracking()
            //     .Select(p => new ProductReadDto
            //     {
            //         ProductId = p.ProductId,
            //         Name = p.Name,
            //         Price = p.Price,
            //         CategoryId = p.CategoryId,
            //         CategoryName = p.Category != null ? p.Category.Name : string.Empty
            //     })
            //     .ToListAsync();
        }

        public async Task<ProductReadDto?> GetProductByIdAsync(Guid productId)
        {
            return await _appDbContext.Products
                .AsNoTracking()
                .Where(p => p.ProductId == productId)
                .Select(p => new ProductReadDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : string.Empty
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProductReadDto?> UpdateProductAsync(Guid productId, ProductUpdateDto updateData)
        {
            // 1. Find product to update
            var product = await _appDbContext.Products.FindAsync(productId);
            if (product == null) return null;

            // 2. Verify the new category exists
            var category = await _appDbContext.Categories.FindAsync(updateData.CategoryId);
            if (category == null) return null;

            // 3. Update entity fields
            product.Name = updateData.Name;
            product.Price = updateData.Price;
            product.StockQuantity = updateData.StockQuantity;
            product.CategoryId = updateData.CategoryId;

            await _appDbContext.SaveChangesAsync();

            return new ProductReadDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = category.Name
            };
        }

        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            var product = await _appDbContext.Products.FindAsync(productId);
            if (product == null) return false;

            // ⚡ Soft Delete instead of hard remove!
            product.IsDeleted = true;
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}