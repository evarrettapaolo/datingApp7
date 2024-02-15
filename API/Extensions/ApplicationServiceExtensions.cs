using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
  public static class ApplicationServiceExtensions
  {
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
      services.AddDbContext<DataContext>(options =>
      {
        options.UseSqlite(config.GetConnectionString("DefaultConnection"));
      });

      services.AddCors(options =>
      {
        options.AddPolicy(name: "MyCors", policy =>
        {
          policy.AllowAnyHeader()
          .AllowAnyMethod()
          .WithOrigins("https://localhost:4200");
        });
      });
      
      services.AddScoped<ITokenService, TokenService>();

      return services;
    }
  }
}