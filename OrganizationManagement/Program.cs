using Microsoft.EntityFrameworkCore;
using OrganizationManagement.Data;
using OrganizationManagement.Controllers;
using OrganizationManagement.IRepository;
using OrganizationManagement.Repository;
using OrganizationManagement.Repository.IRepository;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options=>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapGet("/", () =>
{
    var sayAlive = new
    {
        message = "I'm Alive",
        status = "Okay"
    };
    return Results.Ok(sayAlive);
});
app.MapControllers();

app.Run();

