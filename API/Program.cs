using API;
using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>(); //custom middleware

app.UseHttpsRedirection();

app.UseCors("MyCors");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<PresenceHub>("hubs/presence"); //SignalR endpoint
app.MapHub<MessageHub>("hubs/message"); //SignalR endpoint

//seeding data to database
using var scope = app.Services.CreateScope(); //create scope for services
var services = scope.ServiceProvider; //gather 
try
{
  var context = services.GetRequiredService<DataContext>(); //for exception handling and db insertion
  var userManager = services.GetRequiredService<UserManager<AppUser>>();
  var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
  await context.Database.MigrateAsync(); //create migration by code if not db yet
  await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]"); //clear out message hub tracking table
  await Seed.SeedUsers(userManager, roleManager); 
}
catch (Exception ex)
{
  var logger = services.GetService<ILogger<Program>>();
  logger.LogError(ex, "An error occurred during migration");
}

app.Run();
