﻿using System.ComponentModel.DataAnnotations;
using API.Entities;
using API.Extensions;

namespace API;

public class AppUser
{
  public int Id { get; set; }
  public string UserName { get; set; }
  public byte[] PasswordHash { get; set; }
  public byte[] PasswordSalt { get; set; }
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
  public List<Photo> Photos { get; set; } = new();
  public List<UserLike> LikedByUsers { get; set; } // one to many relation
  public List<UserLike> LikedUsers { get; set; }   //many to one relation
  public List<Message> MessagesSent { get; set; }
  public List<Message> MessagesReceived { get; set; }
}
