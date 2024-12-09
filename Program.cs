using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SalesService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    // Добавление описания для Swagger
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sales Service API",
        Version = "v1",
        Description = "API для управления продажами.",
        Contact = new OpenApiContact
        {
            Name = "Мой GitHub",
            Url = new Uri("https://github.com/nikitronpatron")
        },
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
