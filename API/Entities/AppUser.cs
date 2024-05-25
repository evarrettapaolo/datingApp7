using API.Entities;
using Microsoft.AspNetCore.Identity;

namespace API;

public class AppUser : IdentityUser<int>
{
  public DateOnly DateOfBirth { get; set; }
  public string KnownAs { get; set; }
  public DateTime Created { get; set; } = DateTime.UtcNow;
  public DateTime LastActive { get; set; } = DateTime.UtcNow;
  public string Gender { get; set; }
  public string Introduction { get; set; }
  public string LookingFor { get; set; }
  public string Interests { get; set; }
  public string City { get; set; }
  public string Country { get; set; }
  public List<Photo> Photos { get; set; }
  public List<UserLike> LikedByUsers { get; set; } // one to many relation
  public List<UserLike> LikedUsers { get; set; }   //many to one relation
  public List<Message> MessagesSent { get; set; }
  public List<Message> MessagesReceived { get; set; }
  public ICollection<AppUserRole> UserRoles { get; set; } //identity
}
