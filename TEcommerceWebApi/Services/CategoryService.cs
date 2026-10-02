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

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _appDbContext;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private readonly ICurrentTenantService _currentTenantService; // 👈 Inject CurrentTenantService

    public CategoryService(
        AppDbContext appDbContext, 
        IMapper mapper, 
        ICacheService cacheService,
        ICurrentTenantService currentTenantService)
    {
        _appDbContext = appDbContext;
        _mapper = mapper;
        _cacheService = cacheService;
        _currentTenantService = currentTenantService;
    }

    // Helper to generate tenant-scoped cache prefix:
    private string GetTenantPrefix() => 
        _currentTenantService.TenantId.HasValue 
            ? $"tenant_{_currentTenantService.TenantId.Value}_" 
            : "tenant_global_";

    // 1. GET BY ID
    public async Task<CategoryReadDto?> GetCategoryById(Guid categoryId)
    {
        // 🛡️ Key format: "tenant_nike-guid_category_1111"
        var cacheKey = $"{GetTenantPrefix()}category_{categoryId}";

        var cachedCategory = await _cacheService.GetAsync<CategoryReadDto>(cacheKey);
        if (cachedCategory != null) return cachedCategory;

        var category = await _appDbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null) return null;

        var resultDto = _mapper.Map<CategoryReadDto>(category);
        await _cacheService.SetAsync(cacheKey, resultDto, TimeSpan.FromMinutes(10));

        return resultDto;
    }

    // 2. GET ALL (Paginated)
    public async Task<PaginatedResult<CategoryReadDto>> GetAllCategory(QueryParameters queryParameter)
    {
        // 🛡️ Key format: "tenant_nike-guid_categories_list_p1_s4__"
        var cacheKey = $"{GetTenantPrefix()}categories_list_p{queryParameter.PageNumber}_s{queryParameter.PageSize}_{queryParameter.SearchValue}_{queryParameter.SortOrder}";

        var cachedList = await _cacheService.GetAsync<PaginatedResult<CategoryReadDto>>(cacheKey);
        if (cachedList != null) return cachedList;

        var query = _appDbContext.Categories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParameter.SearchValue))
        {
            var formattedSearch = $"%{queryParameter.SearchValue.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Name, formattedSearch) || 
                                     (c.Description != null && EF.Functions.ILike(c.Description, formattedSearch)));
        }

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

        await _cacheService.SetAsync(cacheKey, paginatedResult, TimeSpan.FromMinutes(5));
        return paginatedResult;
    }

    // 3. CREATE
    public async Task<CategoryReadDto> CreateCategory(CategoryCreateDto categoryData)
    {
        var newCategory = _mapper.Map<Category>(categoryData);
        newCategory.CategoryId = Guid.NewGuid();
        newCategory.CreatedAt = DateTime.UtcNow;

        await _appDbContext.Categories.AddAsync(newCategory);
        await _appDbContext.SaveChangesAsync();

        // 🗑️ Purge ONLY this tenant's list cache:
        await _cacheService.RemoveByPrefixAsync($"{GetTenantPrefix()}categories_list_");

        return _mapper.Map<CategoryReadDto>(newCategory);
    }

    // 4. UPDATE
    public async Task<CategoryReadDto?> UpdateCategory(Guid categoryId, CategoryUpdateDto categoryData)
    {
        var foundCategory = await _appDbContext.Categories.FindAsync(categoryId);
        if (foundCategory == null) return null;

        _mapper.Map(categoryData, foundCategory);
        await _appDbContext.SaveChangesAsync();

        // 🗑️ Invalidate this tenant's single item AND list cache:
        await _cacheService.RemoveAsync($"{GetTenantPrefix()}category_{categoryId}");
        await _cacheService.RemoveByPrefixAsync($"{GetTenantPrefix()}categories_list_");

        return _mapper.Map<CategoryReadDto>(foundCategory);
    }

    // 5. DELETE
    public async Task<bool> DeleteCategoryById(Guid categoryId)
    {
        var foundCategory = await _appDbContext.Categories.FindAsync(categoryId);
        if (foundCategory == null) return false;

        foundCategory.IsDeleted = true;
        await _appDbContext.SaveChangesAsync();

        await _cacheService.RemoveAsync($"{GetTenantPrefix()}category_{categoryId}");
        await _cacheService.RemoveByPrefixAsync($"{GetTenantPrefix()}categories_list_");

        return true;
    }
}