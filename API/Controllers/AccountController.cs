using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  /// <summary>
  /// Authentication controller
  /// </summary>
  public class AccountController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
      _mapper = mapper;
      _tokenService = tokenService;
      _userManager = userManager;
    }

    [HttpPost("register")] // POST: ap/account/user
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
      //Username duplication check
      if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

      var user = _mapper.Map<AppUser>(registerDto);

      user.UserName = registerDto.Username.ToLower();

      var result = await _userManager.CreateAsync(user, registerDto.Password);

      if(!result.Succeeded) return BadRequest(result.Errors);

      var roleResult = await _userManager.AddToRoleAsync(user, "Member");
      
      if(!roleResult.Succeeded) return BadRequest(result.Errors);

      //convert and return the entity object into Dto
      return new UserDto()
      {
        Username = user.UserName,
        Token = await _tokenService.CreateToken(user),
        KnownAs = user.KnownAs,
        Gender = user.Gender
      };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
      //Find username in Db
      var user = await _userManager.Users
        .Include(p => p.Photos) 
        .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

      if (user == null) return Unauthorized("invalid username");

      var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

      if(!result) return Unauthorized("invalid password");

      return new UserDto()
      {
        Username = user.UserName,
        Token = await _tokenService.CreateToken(user),
        PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
        KnownAs = user.KnownAs,
        Gender = user.Gender
      };
    }

    private async Task<bool> UserExists(string username)
    {
      return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
    }
  }
}