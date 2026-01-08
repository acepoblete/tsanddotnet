using Microsoft.EntityFrameworkCore;
using FunctionExecutor.Data;
using FunctionExecutor.Repositories;
using FunctionExecutor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Database - SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=functionexecutor.db"));

// Repositories
builder.Services.AddScoped<ICostCodeRepository, CostCodeRepository>();
builder.Services.AddScoped<IWorkbookRepository, WorkbookRepository>();

// Services
builder.Services.AddScoped<IExcelCalculationService, ExcelCalculationService>();
builder.Services.AddScoped<IWorkbookService, WorkbookService>();

// Controllers
builder.Services.AddControllers();

// CORS - allow React dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Cost Code Workbook API",
        Version = "v1",
        Description = "Execute Excel-based calculations with cost code data"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactDev");

app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Ensure Templates directory exists
var templatesPath = Path.Combine(app.Environment.ContentRootPath, "Templates");
if (!Directory.Exists(templatesPath))
{
    Directory.CreateDirectory(templatesPath);
}

app.Run();
