using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TEcommerceWebApi.data;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Helpers;
using TEcommerceWebApi.Interfaces;
using TEcommerceWebApi.Models;
using TEcommerceWebApi.Controllers;

namespace TEcommerceWebApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public CategoryService(AppDbContext appDbContext, IMapper mapper, ICacheService cacheService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        // =========================================================================
        // 1. GET SINGLE CATEGORY BY ID (Single Entity Caching)
        // =========================================================================
        public async Task<CategoryReadDto?> GetCategoryById(Guid categoryId)
        {
            // Step 1: Create a distinct key for this specific category
            var cacheKey = $"category_{categoryId}";

            // Step 2: Check Redis RAM first
            var cachedCategory = await _cacheService.GetAsync<CategoryReadDto>(cacheKey);
            if (cachedCategory != null)
            {
                return cachedCategory; // ⚡ Cache Hit: Return in ~1ms with 0 database queries!
            }

            // Step 3: Cache Miss -> Query PostgreSQL
            var category = await _appDbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null) return null;

            var resultDto = _mapper.Map<CategoryReadDto>(category);

            // Step 4: Save copy to Redis RAM for 10 minutes
            await _cacheService.SetAsync(cacheKey, resultDto, TimeSpan.FromMinutes(10));

            return resultDto;
        }

        // =========================================================================
        // 2. GET ALL CATEGORIES (Paginated List & Search Caching)
        // =========================================================================
        public async Task<PaginatedResult<CategoryReadDto>> GetAllCategory(QueryParameters queryParameter)
        {
            // Step 1: Build a dynamic key representing the exact page, size, search, and sort parameters
            var cacheKey = $"categories_list_p{queryParameter.PageNumber}_s{queryParameter.PageSize}_{queryParameter.SearchValue}_{queryParameter.SortOrder}";

            // Step 2: Check Redis RAM first
            var cachedList = await _cacheService.GetAsync<PaginatedResult<CategoryReadDto>>(cacheKey);
            if (cachedList != null)
            {
                return cachedList; // ⚡ Cache Hit: Return instantly
            }

            // Step 3: Cache Miss -> Build and execute PostgreSQL query
            var query = _appDbContext.Categories
                .AsNoTracking()
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(queryParameter.SearchValue))
            {
                var formattedSearch = $"%{queryParameter.SearchValue.Trim()}%";
                query = query.Where(c => EF.Functions.ILike(c.Name, formattedSearch) || 
                                         (c.Description != null && EF.Functions.ILike(c.Description, formattedSearch)));
            }

            // Default sorting
            query = query.OrderByDescending(c => c.CreatedAt);

            var totalCategory = await query.CountAsync();
            var items = await query
                .Skip((queryParameter.PageNumber - 1) * queryParameter.PageSize)
                .Take(queryParameter.PageSize)
                .ToListAsync();

            var paginatedResult = new PaginatedResult<CategoryReadDto>
            {
                Items = _mapper.Map<List<CategoryReadDto>>(items),
                TotalCount = totalCategory,
                PageNumber = queryParameter.PageNumber,
                PageSize = queryParameter.PageSize
            };

            // Step 4: Save paginated result in Redis RAM for 5 minutes
            await _cacheService.SetAsync(cacheKey, paginatedResult, TimeSpan.FromMinutes(5));

            return paginatedResult;
        }

        // =========================================================================
        // 3. CREATE CATEGORY (Cache Invalidation)
        // =========================================================================
        public async Task<CategoryReadDto> CreateCategory(CategoryCreateDto categoryData)
        {
            var newCategory = _mapper.Map<Category>(categoryData);
            newCategory.CategoryId = Guid.NewGuid();
            newCategory.CreatedAt = DateTime.UtcNow;

            await _appDbContext.Categories.AddAsync(newCategory);
            await _appDbContext.SaveChangesAsync();

            // 🗑️ Invalidate ALL cached category lists (Page 1, Page 2, searches)
            // The next time any customer browses categories, fresh data will be fetched from DB.
            await _cacheService.RemoveByPrefixAsync("categories_list_");

            return _mapper.Map<CategoryReadDto>(newCategory);
        }

        // =========================================================================
        // 4. UPDATE CATEGORY (Granular Cache Invalidation)
        // =========================================================================
        public async Task<CategoryReadDto?> UpdateCategory(Guid categoryId, CategoryUpdateDto categoryData)
        {
            var foundCategory = await _appDbContext.Categories.FindAsync(categoryId);
            if (foundCategory == null) return null;

            _mapper.Map(categoryData, foundCategory);
            await _appDbContext.SaveChangesAsync();

            // 🗑️ Invalidate this specific item AND all category lists
            await _cacheService.RemoveAsync($"category_{categoryId}");
            await _cacheService.RemoveByPrefixAsync("categories_list_");

            return _mapper.Map<CategoryReadDto>(foundCategory);
        }

        // =========================================================================
        // 5. DELETE CATEGORY (Granular Cache Invalidation)
        // =========================================================================
        public async Task<bool> DeleteCategoryById(Guid categoryId)
        {
            var foundCategory = await _appDbContext.Categories.FindAsync(categoryId);
            if (foundCategory == null) return false;

            // Soft Delete in PostgreSQL
            foundCategory.IsDeleted = true;
            await _appDbContext.SaveChangesAsync();

            // 🗑️ Invalidate this specific item AND all category lists
            await _cacheService.RemoveAsync($"category_{categoryId}");
            await _cacheService.RemoveByPrefixAsync("categories_list_");

            return true;
        }
    }
}