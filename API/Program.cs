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

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

// Gathering db connection string 
var connString = "";
if (builder.Environment.IsDevelopment())
  connString = builder.Configuration.GetConnectionString("DefaultConnection");
else
{
  // Use connection string provided at runtime by FlyIO.
  var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

  // Parse connection URL to connection string for Npgsql
  connUrl = connUrl.Replace("postgres://", string.Empty);
  var pgUserPass = connUrl.Split("@")[0];
  var pgHostPortDb = connUrl.Split("@")[1];
  var pgHostPort = pgHostPortDb.Split("/")[0];
  var pgDb = pgHostPortDb.Split("/")[1];
  var pgUser = pgUserPass.Split(":")[0];
  var pgPass = pgUserPass.Split(":")[1];
  var pgHost = pgHostPort.Split(":")[0];
  var pgPort = pgHostPort.Split(":")[1];
  var updatedHost = pgHost.Replace("flycast", "internal");

  connString = $"Server={updatedHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
}
builder.Services.AddDbContext<DataContext>(opt =>
{
  opt.UseNpgsql(connString);
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>(); //custom middleware


app.UseCors("MyCors");

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles(); //use the index.html in wwwroot
app.UseStaticFiles(); //enable the wwwroot folder


app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence"); //SignalR endpoint
app.MapHub<MessageHub>("hubs/message"); //SignalR endpoint
app.MapFallbackToController("Index", "Fallback");

//seeding data to database
using var scope = app.Services.CreateScope(); //create scope for services
var services = scope.ServiceProvider; //gather 
try
{
  var context = services.GetRequiredService<DataContext>(); //for exception handling and db insertion
  var userManager = services.GetRequiredService<UserManager<AppUser>>();
  var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
  await context.Database.MigrateAsync(); //create migration by code if not db yet
  await Seed.ClearConnections(context); //clear out message hub tracking table
  await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
  var logger = services.GetService<ILogger<Program>>();
  logger.LogError(ex, "An error occurred during migration");
}

app.Run();
