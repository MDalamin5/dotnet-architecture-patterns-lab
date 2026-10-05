using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEcommerceWebApi.data;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Models;
using TEcommerceWebApi.Services;
using TEcommerceWebApi.Interfaces;
using TEcommerceWebApi.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace TEcommerceWebApi.Controllers
{
    [ApiController]
    [Route("/api/v2/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        // Injected Interface
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [Authorize(Policy = Permissions.ProductsCreate)]
        [Consumes("multipart/form-data")] // 👈 Tells Swagger to show the file picker!
        public async Task<ActionResult<ApiResponse<ProductReadDto>>> CreateProduct([FromForm] ProductCreateDto productData)
        {
            var createdProduct = await _productService.CreateProduct(productData);
            if (createdProduct == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Category with ID '{productData.CategoryId}' does not exist." }, 
                    404, 
                    "Validation Failed."
                ));
            }

            return StatusCode(201, ApiResponse<ProductReadDto>.SuccessResponse(createdProduct, 201, "Product created successfully."));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] QueryParameters queryParameters)
        {
            var allProducts = await _productService.GetAllProducts(queryParameters);
            return Ok(ApiResponse<PaginatedResult<ProductReadDto>>.SuccessResponse(allProducts, 200, "All Products returned successfully."));
        }

        //get product by ID
        [HttpGet("{productId:guid}")]
        public async Task<ActionResult<ApiResponse<ProductReadDto>>> GetProductById(Guid productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Product with ID '{productId}' was not found." },
                    404,
                    "Product Not Found."
                ));
            }

            return Ok(ApiResponse<ProductReadDto>.SuccessResponse(product, 200, "Product found."));
        }

        [HttpPut("{productId:guid}")]
        public async Task<ActionResult<ApiResponse<ProductReadDto>>> UpdateProduct(
            Guid productId, 
            [FromBody] ProductUpdateDto updateData)
        {
            var updatedProduct = await _productService.UpdateProductAsync(productId, updateData);
            if (updatedProduct == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Product or Category not found with the provided IDs." },
                    404,
                    "Update Failed."
                ));
            }

            return Ok(ApiResponse<ProductReadDto>.SuccessResponse(updatedProduct, 200, "Product updated successfully."));
        }

        [HttpDelete("{productId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid productId)
        {
            var deleted = await _productService.DeleteProductAsync(productId);
            if (!deleted)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"Product with ID '{productId}' was not found." },
                    404,
                    "Delete Failed."
                ));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, 200, "Product deleted successfully."));
        }
    }
}

