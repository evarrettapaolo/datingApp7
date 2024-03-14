using API.Data;
using API.Entities;
using API.Helpers;
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
      services.AddScoped<IUserRepository, UserRepository>();
      services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
      services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
      services.AddScoped<IPhotoService, PhotoServices>();
      services.AddScoped<LogUserActivity>();
      services.AddScoped<ILikesRepository, LikesRepository>();

      return services;
    }
  }
}