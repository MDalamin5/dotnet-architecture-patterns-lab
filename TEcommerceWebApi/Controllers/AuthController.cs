using System.Collections.Generic;
using System.Threading.Tasks;
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

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] AuthRegisterDto registerData)
        {
            var response = await _authService.RegisterAsync(registerData);
            if (response == null)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(
                    new List<string> { $"User with email '{registerData.Email}' already exists." },
                    409,
                    "Registration Failed"
                ));
            }

            return StatusCode(201, ApiResponse<AuthResponseDto>.SuccessResponse(response, 201, "User registered successfully."));
        }

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
    }
}