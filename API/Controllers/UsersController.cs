using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
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
  private readonly IMapper _mapper;
  private readonly IPhotoService _photoService;
  private readonly IUnitOfWork _uow;

  public UsersController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
  {
    _uow = uow;
    _mapper = mapper;
    _photoService = photoService;
  }

  // [Authorize(Roles = "Admin")] //for testing
  [HttpGet]
  public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
  {
    //utilize the repo and principal claim classes
    var gender = await _uow.UserRepository.GetUserGender(User.GetUsername());
    userParams.CurrentUsername = User.GetUsername();
    //default gender filtering 
    if (string.IsNullOrEmpty(userParams.Gender))
    {
      userParams.Gender = gender == "male" ? "female" : "male";
    }
    var users = await _uow.UserRepository.GetMembersAsync(userParams);
    //populate the custom made response headers
    Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
    //return the list as response
    return Ok(users);
  }

  // [Authorize(Roles = "Member")] //for testing
  [HttpGet("{username}")]
  public async Task<ActionResult<MemberDto>> GetUser(string username)
  {
    return Ok(await _uow.UserRepository.GetMemberAsync(username));
  }

  [HttpPut]
  public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
  {
    //db query
    var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

    if (user == null) return NotFound();

    //from dto to entity
    _mapper.Map(memberUpdateDto, user); //update the properties

    if (await _uow.Complete()) return NoContent();

    return BadRequest("Failed to update user"); //failed if reached this part

  }

  [HttpPost("add-photo")]
  public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
  {
    //gather the user object
    var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

    if (user == null) return NotFound(); //no object

    //upload to cloudinary, store image
    var result = await _photoService.AddPhotoAsync(file);

    if (result.Error != null) return BadRequest(result.Error.Message); //failed upload

    //object to append in the User photos array
    var photo = new Photo
    {
      Url = result.SecureUrl.AbsoluteUri, //cloudinary storage link
      PublicId = result.PublicId
    };

    if (user.Photos.Count == 0) photo.IsMain = true; //set main if it is first photo

    user.Photos.Add(photo); //append

    if (await _uow.Complete())
    {
      //return a 201 - created, with a photoDto object returned
      return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
    }

    return BadRequest("Problem adding photo"); //
  }

  [HttpPut("set-main-photo/{photoId}")]
  public async Task<ActionResult> SetMainPhoto(int photoId)
  {
    //get current user
    var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

    if (user == null) return NotFound(); //user not found

    //get the matching photo based on id
    var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

    if (photo == null) return NotFound(); //photo not found

    if (photo.IsMain) return BadRequest("this is already you main photo"); //already main

    //get the current photo
    var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

    if (currentMain != null) currentMain.IsMain = false; //set the current main to false

    photo.IsMain = true; //set the new matched photo to true

    if (await _uow.Complete()) return NoContent(); //save db

    return BadRequest("Problem setting the main photo");
  }

  [HttpDelete("delete-photo/{photoId}")]
  public async Task<ActionResult> DeletePhoto(int photoId)
  {
    //null is not possible, since this controller requires authentication
    var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

    var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);  //store matching photo

    if (photo == null) return NotFound(); //photo not found

    if (photo.IsMain) return BadRequest("You cannot delete your main photo");

    if (photo.PublicId != null) //delete attempt with exception handling
    {
      var result = await _photoService.DeletePhotoAsync(photo.PublicId);
      if (result.Error != null) return BadRequest(result.Error.Message);
    }

    user.Photos.Remove(photo); //delete from repo object

    if (await _uow.Complete()) return Ok(); //apply changes in repo

    return BadRequest("Problem deleting photo");
  }

}
