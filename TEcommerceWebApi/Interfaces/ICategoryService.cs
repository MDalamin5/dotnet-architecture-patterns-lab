using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Controllers;
using TEcommerceWebApi.Helpers;
using TEcommerceWebApi.Models;

namespace TEcommerceWebApi.Interfaces
{
    public interface ICategoryService //:IGenericRepository<Category>
    {
        Task<PaginatedResult<CategoryReadDto>> GetAllCategory(QueryParameters queryParameter);
        Task<CategoryReadDto?> GetCategoryById(Guid categoryId);
        Task<CategoryReadDto> CreateCategory(CategoryCreateDto categoryData);
        Task<CategoryReadDto?> UpdateCategory(Guid categoryId, CategoryUpdateDto categoryData);
        Task<bool> DeleteCategoryById(Guid categoryId);
    }
}