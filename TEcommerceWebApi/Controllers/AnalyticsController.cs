using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Controllers
{
    [ApiController]
    [Route("/api/v2/analytics")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("category-summary")]
        public async Task<ActionResult<ApiResponse<List<CategorySummaryDto>>>> GetCategorySummaries()
        {
            var summaries = await _analyticsService.GetCategorySummariesAsync();
            return Ok(ApiResponse<List<CategorySummaryDto>>.SuccessResponse(summaries, 200, "Category summaries retrieved."));
        }

        [HttpGet("top-selling-products")]
        public async Task<ActionResult<ApiResponse<List<TopSellingProductDto>>>> GetTopSellingProducts([FromQuery] int count = 5)
        {
            var topProducts = await _analyticsService.GetTopSellingProductsAsync(count);
            return Ok(ApiResponse<List<TopSellingProductDto>>.SuccessResponse(topProducts, 200, "Top selling products retrieved."));
        }

        [HttpGet("customer-spending")]
        public async Task<ActionResult<ApiResponse<List<CustomerSpendingDto>>>> GetCustomerSpending([FromQuery] decimal minSpent = 0)
        {
            var result = await _analyticsService.GetCustomerSpendingSummaryAsync(minSpent);
            return Ok(ApiResponse<List<CustomerSpendingDto>>.SuccessResponse(result, 200, "Customer spending summary retrieved."));
        }

        [HttpGet("sales-overview")]
        public async Task<ActionResult<ApiResponse<SalesOverviewDto>>> GetSalesOverview()
        {
            var overview = await _analyticsService.GetSalesOverviewAsync();
            return Ok(ApiResponse<SalesOverviewDto>.SuccessResponse(overview, 200, "Executive sales overview retrieved."));
        }
    }
}