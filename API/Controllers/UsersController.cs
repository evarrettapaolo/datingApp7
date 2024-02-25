using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Business logics and functionalities
/// </summary>
[Authorize]
public class UsersController : BaseApiController
{
  private readonly IUserRepository _userRepository;
  private readonly IMapper _mapper;
  private readonly IPhotoService _photoService;

  public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
  {
    _mapper = mapper;
    _photoService = photoService;
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
    //db query
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

    if (user == null) return NotFound();

    //from dto to entity
    _mapper.Map(memberUpdateDto, user); //update the properties

    if (await _userRepository.SaveAllAsync()) return NoContent();

    return BadRequest("Failed to update user"); //failed if reached this part

  }

  [HttpPost("add-photo")]
  public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
  {
    //gather the user object
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

    if(user == null) return NotFound(); //no object

    //upload to cloudinary, store image
    var result = await _photoService.AddPhotoAsync(file); 

    if(result.Error != null) return BadRequest(result.Error.Message); //failed upload

    //object to append in the User photos array
    var photo = new Photo
    {
      Url = result.SecureUrl.AbsoluteUri, //cloudinary storage link
      PublicId = result.PublicId
    };

    if(user.Photos.Count == 0) photo.IsMain = true; //set main if it is first photo

    user.Photos.Add(photo); //append

    if(await _userRepository.SaveAllAsync()) 
    {
      //return a 201 - created, with a photoDto object returned
      return CreatedAtAction(nameof(GetUser), new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
    }

    return BadRequest("Problem adding photo"); //
  }

  [HttpPut("set-main-photo/{photoId}")]
  public async Task<ActionResult> SetMainPhoto(int photoId)
  {
    //get current user
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

    if(user == null) return NotFound(); //user not found

    //get the matching photo based on id
    var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

    if(photo == null) return NotFound(); //photo not found

    if(photo.IsMain) return BadRequest("this is already you main photo"); //already main

    //get the current photo
    var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

    if(currentMain != null) currentMain.IsMain = false; //set the current main to false

    photo.IsMain = true; //set the new matched photo to true

    if(await _userRepository.SaveAllAsync()) return NoContent(); //save db

    return BadRequest("Problem setting the main photo");
  }

  [HttpDelete("delete-photo/{photoId}")]
  public async Task<ActionResult> DeletePhoto(int photoId)
  {
    //null is not possible, since this controller requires authentication
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

    var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);  //store matching photo

    if(photo == null) return NotFound(); //photo not found

    if(photo.IsMain) return BadRequest("You cannot delete your main photo");

    if(photo.PublicId != null) //delete attempt with exception handling
    {
      var result = await _photoService.DeletePhotoAsync(photo.PublicId);
      if(result.Error != null) return BadRequest(result.Error.Message);
    }

    user.Photos.Remove(photo); //delete from repo object
    
    if(await _userRepository.SaveAllAsync()) return Ok(); //apply changes in repo

    return BadRequest("Problem deleting photo");
  }

}
