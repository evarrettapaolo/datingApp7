using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class Seed
  {
    public static async Task SeedUsers(DataContext context)
    {
      //check if there are entries in database
      if (await context.Users.AnyAsync()) return;

      //take object data from json
      var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

      //make sure the json has the same casing format
      var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

      var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

      //build each user entry
      foreach (var user in users)
      {
        using var hmac = new HMACSHA512();
        user.UserName = user.UserName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
        user.PasswordSalt = hmac.Key;

        context.Users.Add(user);
      }
      await context.SaveChangesAsync();
    }
  }
}