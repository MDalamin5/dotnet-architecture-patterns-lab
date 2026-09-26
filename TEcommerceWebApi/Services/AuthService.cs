using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TEcommerceWebApi.data;
using TEcommerceWebApi.DTOs;
using TEcommerceWebApi.Enums;
using TEcommerceWebApi.Interfaces;
using TEcommerceWebApi.Models;

namespace TEcommerceWebApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext appDbContext, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(AuthRegisterDto registerData)
        {
            // 1. Check if email already exists
            var emailExists = await _appDbContext.Users.AnyAsync(u => u.Email.ToLower() == registerData.Email.ToLower());
            if (emailExists) return null;

            // 2. Hash Password using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerData.Password);

            // 3. Create User Entity
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = registerData.Email.Trim().ToLower(),
                FullName = registerData.FullName.Trim(),
                PasswordHash = passwordHash,
                Role = registerData.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            // 4. Generate JWT Token
            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(AuthLoginDto loginData)
        {
            // 1. Find User by Email
            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginData.Email.Trim().ToLower());

            if (user == null) return null;

            // 2. Verify Password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginData.Password, user.PasswordHash);
            if (!isPasswordValid) return null;

            // 3. Generate JWT Token with Claims
            return GenerateAuthResponse(user);
        }

        // 🔑 The Helper that creates Claims and signs the JWT Token:
        private AuthResponseDto GenerateAuthResponse(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = Convert.ToDouble(jwtSettings["ExpiryMinutes"] ?? "120");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 🛡️ THESE ARE THE CLAIMS!
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // 👈 This is what [Authorize(Roles = "...")] checks!
            };

            var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(tokenDescriptor);

            return new AuthResponseDto
            {
                Token = tokenString,
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                ExpiresAt = expires
            };
        }
    }
}