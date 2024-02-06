using API;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(options =>
{
  options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors(options =>
{
  options.AddPolicy(name: "MyCors", policy =>
  {
    policy.AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("https://localhost:4200");
  });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("MyCors");

app.MapControllers();

app.Run();
