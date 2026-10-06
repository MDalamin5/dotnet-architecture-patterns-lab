using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Controllers
{
    [ApiController]
    [Route("/api/v2/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Register Account
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] AuthRegisterDto registerData)
        {
            try
            {
                var response = await _authService.RegisterAsync(registerData);
                if (response == null)
                {
                    return Conflict(ApiResponse<object>.ErrorResponse(
                        new List<string> { $"User with email '{registerData.Email}' already exists in this store." },
                        409,
                        "Registration Failed"
                    ));
                }

                return StatusCode(201, ApiResponse<AuthResponseDto>.SuccessResponse(response, 201, "User registered successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, 400, "Store Required"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(new List<string> { ex.Message }, 404, "Store Not Found"));
            }
        }

        // Login Account
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] AuthLoginDto loginData)
        {
            var response = await _authService.LoginAsync(loginData);
            if (response == null)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    new List<string> { "Invalid email or password." },
                    401,
                    "Authentication Failed"
                ));
            }

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, 200, "Login successful."));
        }

        // View Current User Profile & Active Permissions
        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponse<object>> GetCurrentUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role);
            var tenantId = User.FindFirstValue("TenantId");
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();

            var userProfile = new
            {
                UserId = userId,
                TenantId = tenantId,
                Email = email,
                FullName = name,
                Role = role,
                Permissions = permissions
            };

            return Ok(ApiResponse<object>.SuccessResponse(userProfile, 200, "Current user profile retrieved."));
        }
    }
}