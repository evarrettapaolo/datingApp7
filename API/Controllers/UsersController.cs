using System.Security.Claims;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
  private readonly IUserRepository _userRepository;
  private readonly IMapper _mapper;

  public UsersController(IUserRepository userRepository, IMapper mapper)
  {
    _mapper = mapper;
    _userRepository = userRepository;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
  {
    return Ok(await _userRepository.GetMembersAsync());
  }

  [HttpGet("{username}")]
  public async Task<ActionResult<MemberDto>> GetUser(string username)
  {
    return Ok(await _userRepository.GetMemberAsync(username));
  }

  [HttpPut]
  public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
  {
    //gather current stored User identity
    var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //db query
    var user = await _userRepository.GetUserByUsernameAsync(username);

    if(user == null) return NotFound(); 

    //from dto to entity
    _mapper.Map(memberUpdateDto, user); //update the properties

    if(await _userRepository.SaveAllAsync()) return NoContent();

    return BadRequest("Failed to update user"); //failed if reached this part

  }
}
