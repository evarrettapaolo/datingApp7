using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AdminController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _uow;
    private readonly IPhotoService _photoService;
    public AdminController(UserManager<AppUser> userManager, IUnitOfWork uow, IPhotoService photoService)
    {
      _uow = uow;
      _userManager = userManager;
      _photoService = photoService;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
      var user = await _userManager.Users
        .OrderBy(u => u.UserName)
        .Select(u => new 
        { 
          u.Id,
          Username = u.UserName,
          Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
        })
        .ToListAsync();

      return Ok(user);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, [FromQuery]string roles)
    {
      if(string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

      var selectedRoles = roles.Split(",").ToArray(); //new roles

      var user = await _userManager.FindByNameAsync(username); //get user

      if(user == null) return NotFound();

      var userRoles = await _userManager.GetRolesAsync(user); //user's current roles

      //add, only add what's not present already on the new role list
      var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

      if(!result.Succeeded) return BadRequest("Failed to add to roles");

      //delete, only delete what's not on the new role list
      result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

      if(!result.Succeeded) return BadRequest("Failed to remove from roles");

      return Ok(await _userManager.GetRolesAsync(user)); //send updated
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult> GetPhotosForModeration()
    {
      var listOfUnapprovedPhotos = await _uow.PhotoRepository.GetUnapprovedPhotos();

      return Ok(listOfUnapprovedPhotos);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
      //approving
      var photo = await _uow.PhotoRepository.GetPhotoById(photoId);

      if(photo == null) return NotFound("Could not find photo");

      photo.IsApproved = true;

      //setting main
      var user = await _uow.UserRepository.GetUserByPhotoId(photoId);

      if(!user.Photos.Any(p => p.IsMain)) photo.IsMain = true;

      await _uow.Complete();

      return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
      var photo = await _uow.PhotoRepository.GetPhotoById(photoId);

      if(photo == null) return NotFound("Could not find photo");

      if(photo.PublicId != null)
      {
        var result = await _photoService.DeletePhotoAsync(photo.PublicId);

        if(result.Result == "ok")
        {
          _uow.PhotoRepository.RemovePhoto(photo);
        }
      }
      else //cloudinary not giving publicId, image possible don't exist on cloud
      {
        _uow.PhotoRepository.RemovePhoto(photo); //delete image uri on db
      }

      await _uow.Complete();

      return Ok();
    }
  }
}