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
using TEcommerceWebApi.Interfaces;
using TEcommerceWebApi.Models;

namespace TEcommerceWebApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        private readonly ICurrentTenantService _currentTenantService;

        public AuthService(
            AppDbContext appDbContext, 
            IConfiguration configuration,
            ICurrentTenantService currentTenantService)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
            _currentTenantService = currentTenantService;
        }

        public async Task<AuthResponseDto?> RegisterAsync(AuthRegisterDto registerData)
        {
            // 1. Ensure a Store/Tenant is resolved
            if (!_currentTenantService.TenantId.HasValue)
            {
                throw new InvalidOperationException("Cannot register without an active store context. Provide a valid store subdomain or X-Tenant-Id header.");
            }

            var tenantId = _currentTenantService.TenantId.Value;
            var emailLower = registerData.Email.Trim().ToLower();

            // 2. Check if email already exists IN THIS SPECIFIC STORE!
            var emailExistsInStore = await _appDbContext.Users
                .AnyAsync(u => u.Email.ToLower() == emailLower && u.TenantId == tenantId);

            if (emailExistsInStore) return null;

            // 3. Find Role
            var roleName = registerData.Role.ToString();
            var role = await _appDbContext.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower())
                ?? await _appDbContext.Roles.FirstAsync(r => r.Name == "Customer");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerData.Password);

            // 4. Create User scoped to this Store
            var user = new User
            {
                UserId = Guid.NewGuid(),
                TenantId = tenantId, // 👈 Scoped to this store!
                Email = emailLower,
                FullName = registerData.FullName.Trim(),
                PasswordHash = passwordHash,
                RoleId = role.RoleId,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(AuthLoginDto loginData)
        {
            // Ensure a store is identified
            if (!_currentTenantService.TenantId.HasValue)
            {
                throw new InvalidOperationException("Cannot login without an active store context. Provide a valid store subdomain or X-Tenant-Id header.");
            }

            var tenantId = _currentTenantService.TenantId.Value;
            var emailLower = loginData.Email.Trim().ToLower();

            // Find user in THIS store only (EF Core query filter handles TenantId automatically)
            var user = await _appDbContext.Users
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower && u.TenantId == tenantId);

            if (user == null) return null;

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginData.Password, user.PasswordHash);
            if (!isPasswordValid) return null;

            return GenerateAuthResponse(user);
        }

        private AuthResponseDto GenerateAuthResponse(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = Convert.ToDouble(jwtSettings["ExpiryMinutes"] ?? "120");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role != null ? user.Role.Name : "Customer"),
                new Claim("TenantId", user.TenantId.ToString()) // 👈 Tenant identity inside token!
            };

            // Dynamic permissions from Role
            if (user.Role?.RolePermissions != null)
            {
                foreach (var rp in user.Role.RolePermissions)
                {
                    if (rp.Permission != null && !string.IsNullOrWhiteSpace(rp.Permission.Code))
                    {
                        claims.Add(new Claim("Permission", rp.Permission.Code));
                    }
                }
            }

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
                Role = user.Role?.Name ?? "Customer",
                ExpiresAt = expires
            };
        }
    }
}