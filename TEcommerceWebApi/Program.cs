using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StackExchange.Redis;
using TEcommerceWebApi.Controllers;
using TEcommerceWebApi.data;
using TEcommerceWebApi.Helpers;
using TEcommerceWebApi.Interfaces;
using TEcommerceWebApi.Middlewares;
using TEcommerceWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

// 1. Swagger Configuration with JWT + X-Tenant-Id Header
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddControllers();

// 2. Register Application & Tenant Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>(); // 👈 Injected Scoped per request
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Redis
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379,abortConnect=false";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "TEcommerce_";
});
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Auth Setup
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Permissions.CategoriesCreate, policy => policy.RequireClaim("Permission", Permissions.CategoriesCreate));
    options.AddPolicy(Permissions.CategoriesUpdate, policy => policy.RequireClaim("Permission", Permissions.CategoriesUpdate));
    options.AddPolicy(Permissions.CategoriesDelete, policy => policy.RequireClaim("Permission", Permissions.CategoriesDelete));
    options.AddPolicy(Permissions.ProductsCreate, policy => policy.RequireClaim("Permission", Permissions.ProductsCreate));
    options.AddPolicy(Permissions.ProductsUpdate, policy => policy.RequireClaim("Permission", Permissions.ProductsUpdate));
    options.AddPolicy(Permissions.ProductsDelete, policy => policy.RequireClaim("Permission", Permissions.ProductsDelete));
    options.AddPolicy(Permissions.AnalyticsView, policy => policy.RequireClaim("Permission", Permissions.AnalyticsView));
    options.AddPolicy(Permissions.OrdersCreate, policy => policy.RequireClaim("Permission", Permissions.OrdersCreate));
    options.AddPolicy(Permissions.OrdersManageStatus, policy => policy.RequireClaim("Permission", Permissions.OrdersManageStatus));
});

var app = builder.Build();

// 🛡️ Middleware Pipeline Order
app.UseMiddleware<GlobalExceptionMiddleware>();

// ⚡ RESOLVE TENANT BEFORE AUTHENTICATION & CONTROLLERS
app.UseMiddleware<TenantResolutionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();