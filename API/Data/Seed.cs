using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class Seed
  {
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
      //check if there are entries in database
      if (await userManager.Users.AnyAsync()) return;

      //take object data from json
      var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

      var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

      var roles = new List<AppRole>()
      {
        new AppRole(){Name = "Member"},
        new AppRole(){Name = "Admin"},
        new AppRole(){Name = "Moderator"},
      };

      foreach (var role in roles)
      {
        await roleManager.CreateAsync(role);
      }

      //build each user entry
      foreach (var user in users)
      {
        user.UserName = user.UserName.ToLower();
        await userManager.CreateAsync(user, "Pa$$w0rd"); //create and save changes to db
        await userManager.AddToRoleAsync(user, "Member");
      }

      //create an admin account
      var admin = new AppUser()
      {
        UserName = "admin"
      };

      await userManager.CreateAsync(admin, "Pa$$w0rd");
      await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
    }
  }
}