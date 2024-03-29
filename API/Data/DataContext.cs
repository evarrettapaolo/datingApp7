﻿using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API;

public class DataContext : DbContext
{
  public DataContext(DbContextOptions<DataContext> options) : base(options)
  {
  }

  public DbSet<AppUser> Users { get; set; }
  public DbSet<UserLike> Likes { get; set; }
  public DbSet<Message> Messages { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

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
