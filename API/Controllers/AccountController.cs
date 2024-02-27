using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  /// <summary>
  /// Authentication controller
  /// </summary>
  public class AccountController : BaseApiController
  {
    private readonly DataContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
    {
      _mapper = mapper;
      _tokenService = tokenService;
      _context = context;
    }

    [HttpPost("register")] // POST: ap/account/user
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
      //Username duplication check
      if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

      var user = _mapper.Map<AppUser>(registerDto);

      using var hmac = new HMACSHA512(); //class disposal guaranteed with using

      //initialize appuser object
      user.UserName = registerDto.Username.ToLower();
      user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
      user.PasswordSalt = hmac.Key;

      //add the entity object to Db and save
      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      //convert and return the entity object into Dto
      return new UserDto()
      {
        Username = user.UserName,
        Token = _tokenService.CreateToken(user),
        KnownAs = user.KnownAs
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      //Find username in Db
      var user = await _context.Users
        .Include(p => p.Photos) //required to access the main photo url value
        .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

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
        Token = _tokenService.CreateToken(user),
        PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
        KnownAs = user.KnownAs
      };
    }

    private async Task<bool> UserExists(string username)
    {
      return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
  }
}