using API.Data;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
  public static class ApplicationServiceExtensions
  {
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
      services.AddControllers();

      services.AddCors(options =>
      {
        options.AddPolicy(name: "MyCors", policy =>
        {
          policy.AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials() //Allows SignalR
          .WithOrigins("http://localhost:4200", "https://localhost:4200");
        });
      });

      services.AddScoped<ITokenService, TokenService>();
      services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
      services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
      services.AddScoped<IPhotoService, PhotoServices>();
      services.AddScoped<LogUserActivity>();
      services.AddSignalR();
      services.AddSingleton<PresenceTracker>();
      services.AddScoped<IUnitOfWork, UnitOfWork>();

      return services;
    }
  }
}