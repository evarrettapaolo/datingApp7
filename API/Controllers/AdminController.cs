using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
  public class AdminController : BaseApiController
  {
    private readonly UserManager<AppUser> _userManager;

    public AdminController(UserManager<AppUser> userManager)
    {
      _userManager = userManager;
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
    public ActionResult GetPhotosForModeration()
    {
      return Ok("Admins or moderators can see this");
    }
  }
}