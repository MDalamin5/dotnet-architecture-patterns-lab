using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Helpers;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Controllers
{
    [ApiController]
    [Route("/api/v2/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        // Injected only the Service interface
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // 1. Get All Categories (with Pagination, Search & Sort)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResult<CategoryReadDto>>>> GetCategories(
            [FromQuery] QueryParameters queryParameters)
        {
            queryParameters.Validate();
            var result = await _categoryService.GetAllCategory(queryParameters);
            return Ok(ApiResponse<PaginatedResult<CategoryReadDto>>.SuccessResponse(
                result, 
                200, 
                "Categories retrieved successfully."
            ));
        }

        // 2. Get Category By ID
        [HttpGet("{categoryId:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryReadDto>>> GetCategoryById(Guid categoryId)
        {
            var category = await _categoryService.GetCategoryById(categoryId);

            if (category == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Category with ID '{categoryId}' was not found." }, 
                    404, 
                    "Category Not Found"
                ));
            }

            return Ok(ApiResponse<CategoryReadDto>.SuccessResponse(category, 200, "Category found."));
        }

        // 3. Create Category
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryReadDto>>> CreateCategory(
            [FromBody] CategoryCreateDto categoryData)
        {
            var createdCategory = await _categoryService.CreateCategory(categoryData);

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { categoryId = createdCategory.CategoryId },
                ApiResponse<CategoryReadDto>.SuccessResponse(createdCategory, 201, "Category created successfully.")
            );
        }

        // 4. Update Category
        [HttpPut("{categoryId:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryReadDto>>> UpdateCategoryById(
            Guid categoryId, 
            [FromBody] CategoryUpdateDto categoryData)
        {
            var updatedCategory = await _categoryService.UpdateCategory(categoryId, categoryData);

            if (updatedCategory == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Category with ID '{categoryId}' was not found." }, 
                    404, 
                    "Update Failed"
                ));
            }

            return Ok(ApiResponse<CategoryReadDto>.SuccessResponse(updatedCategory, 200, "Category updated successfully."));
        }

        // 5. Delete Category (Soft Delete)
        [HttpDelete("{categoryId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCategoryById(Guid categoryId)
        {
            var deleted = await _categoryService.DeleteCategoryById(categoryId);

            if (!deleted)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Category with ID '{categoryId}' was not found." }, 
                    404, 
                    "Delete Failed"
                ));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, 200, "Category deleted successfully."));
        }
    }
}