using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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

        // =========================================================================
        // 1. REGISTER: Auto-detects store or uses provided tenant
        // =========================================================================
        public async Task<AuthResponseDto?> RegisterAsync(AuthRegisterDto registerData)
        {
            Guid tenantId;

            // Strategy 1: Tenant resolved from Header (X-Tenant-Id) or Subdomain
            if (_currentTenantService.TenantId.HasValue)
            {
                tenantId = _currentTenantService.TenantId.Value;
            }
            // Strategy 2: Tenant passed inside JSON body
            else if (registerData.TenantId.HasValue)
            {
                tenantId = registerData.TenantId.Value;
            }
            // Strategy 3: Development fallback on localhost -> auto-pick first store from DB
            else
            {
                var defaultTenant = await _appDbContext.Tenants.FirstOrDefaultAsync();
                if (defaultTenant == null)
                {
                    throw new InvalidOperationException("No stores exist yet. Please create a store first via POST /api/v2/tenants.");
                }
                tenantId = defaultTenant.TenantId;
            }

            // Verify the store exists
            var tenantExists = await _appDbContext.Tenants.AnyAsync(t => t.TenantId == tenantId);
            if (!tenantExists)
            {
                throw new KeyNotFoundException($"Store with ID '{tenantId}' was not found.");
            }

            var emailLower = registerData.Email.Trim().ToLower();

            // Check if email already exists IN THIS SPECIFIC STORE
            var emailExistsInStore = await _appDbContext.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email.ToLower() == emailLower && u.TenantId == tenantId);

            if (emailExistsInStore) return null;

            // Find matching Role (Admin or Customer)
            var roleName = registerData.Role.ToString();
            var role = await _appDbContext.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());

            if (role == null)
            {
                role = await _appDbContext.Roles
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Name == "Customer");

                if (role == null) throw new InvalidOperationException("Default role not found in database.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerData.Password);

            // Create User linked to this specific store
            var user = new User
            {
                UserId = Guid.NewGuid(),
                TenantId = tenantId,
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

        // =========================================================================
        // 2. LOGIN: Works WITH or WITHOUT tenant header
        // =========================================================================
        public async Task<AuthResponseDto?> LoginAsync(AuthLoginDto loginData)
        {
            var emailLower = loginData.Email.Trim().ToLower();

            // Case A: Store is known (via header or subdomain)
            if (_currentTenantService.TenantId.HasValue)
            {
                var tenantId = _currentTenantService.TenantId.Value;

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

            // Case B: No store header passed (localhost testing / merchant global login)
            var candidateUsers = await _appDbContext.Users
                .IgnoreQueryFilters()
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .Where(u => u.Email.ToLower() == emailLower)
                .ToListAsync();

            if (candidateUsers.Count == 0) return null;

            var matchedUser = candidateUsers.FirstOrDefault(u => 
                BCrypt.Net.BCrypt.Verify(loginData.Password, u.PasswordHash));

            if (matchedUser == null) return null;

            return GenerateAuthResponse(matchedUser);
        }

        // =========================================================================
        // 3. GENERATE TOKEN WITH CLAIMS
        // =========================================================================
        private AuthResponseDto GenerateAuthResponse(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = Convert.ToDouble(jwtSettings["ExpiryMinutes"] ?? "120");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role != null ? user.Role.Name : "Customer"),
                new Claim("TenantId", user.TenantId.ToString())
            };

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