using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyRestaurantAPI;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-AU");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDBContext>((DbContextOptionsBuilder options) => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ));
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "MyRestaurant Reservation API", 
        Version = "v1",
        Description = "A simple test Web API assigned by Barrington Group",
        Contact = new OpenApiContact { Name = "Bony Limas", Email = "bony.limas@gmail.com", Url = new Uri("https://www.linkedin.com/in/bonny-limas-13a41322b/") },
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
