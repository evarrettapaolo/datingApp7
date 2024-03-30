using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API;

public class DataContext : IdentityDbContext<AppUser, AppRole, int, 
IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, 
IdentityRoleClaim<int>, IdentityUserToken<int>>
{
  public DataContext(DbContextOptions<DataContext> options) : base(options)
  {
  }

  public DbSet<UserLike> Likes { get; set; }
  public DbSet<Message> Messages { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    //identity
    modelBuilder.Entity<AppUser>()
      .HasMany(ur => ur.UserRoles)
      .WithOne(u => u.User)
      .HasForeignKey(ur => ur.UserId)
      .IsRequired();

    modelBuilder.Entity<AppRole>()
      .HasMany(ur => ur.UserRoles)
      .WithOne(u => u.Role)
      .HasForeignKey(ur => ur.RoleId)
      .IsRequired();

    //primary key
    modelBuilder.Entity<UserLike>()
      .HasKey(k => new { k.SourceUserId, k.TargetUserId });

    //relation
    //current user can like other users
    modelBuilder.Entity<UserLike>()
      .HasOne(s => s.SourceUser)
      .WithMany(l => l.LikedUsers)
      .HasForeignKey(s => s.SourceUserId)
      .OnDelete(DeleteBehavior.Cascade);

    //current user liked by other users
    modelBuilder.Entity<UserLike>()
      .HasOne(s => s.TargetUser)
      .WithMany(l => l.LikedByUsers)
      .HasForeignKey(s => s.TargetUserId)
      .OnDelete(DeleteBehavior.Cascade);

    //message feature, many to many relation
    modelBuilder.Entity<Message>()
      .HasOne(u => u.Recipient)
      .WithMany(m => m.MessagesReceived)
      .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Message>()
      .HasOne(u => u.Sender)
      .WithMany(m => m.MessagesSent)
      .OnDelete(DeleteBehavior.Restrict);
  }

}
