using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AccountController : BaseApiController
  {
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    public AccountController(DataContext context, ITokenService tokenService)
    {
      _tokenService = tokenService;
      _context = context;
    }

    [HttpPost("register")] // POST: ap/account/user
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
    {
      //Username duplication check
      if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

      using var hmac = new HMACSHA512(); //class disposal guaranteed with using

      //create an entity object
      var user = new AppUser
      {
        UserName = registerDto.Username.ToLower(),
        PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        PasswordSalt = hmac.Key
      };

      //add the entity object to Db and save
      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      //convert and return the entity object into Dto
      return new UserDto()
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      //Find username in Db
      var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.Username);

      //Check if we found one
      if (user == null) return Unauthorized("invalid username");

      //Get the Salt
      using var hmac = new HMACSHA512(user.PasswordSalt);

      //Compute hash using salt and password value 
      var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

      //compare each value
      for (int i = 0; i < computedHash.Length; i++)
      {
        if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("invalid password");
      }

      //success, send the DTO object
      return new UserDto()
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user)
      };
    }

    private async Task<bool> UserExists(string username)
    {
      return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
  }
}