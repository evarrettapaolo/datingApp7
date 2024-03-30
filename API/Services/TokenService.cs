using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
  public class TokenService : ITokenService
  {
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;
    private readonly UserManager<AppUser> _userManager;
    public TokenService(IConfiguration config, UserManager<AppUser> userManager)
    {
      _userManager = userManager;
      _config = config;
      _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]));
    }

    public async Task<string> CreateToken(AppUser user)
    {
      //claims or payload value
      var claims = new List<Claim>
      {
        new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
      };

      //validate user role
      var roles = await _userManager.GetRolesAsync(user);

      //add role(s) to the claim list
      claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

      //object for generating signature, using key
      var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

      //token object, put def together
      var tokenDescriptor = new SecurityTokenDescriptor()
      {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.Now.AddDays(7),
        SigningCredentials = creds
      };

      var TokenHandler = new JwtSecurityTokenHandler();

      //generate the token
      var token = TokenHandler.CreateToken(tokenDescriptor);

      return TokenHandler.WriteToken(token);
    }
  }
}