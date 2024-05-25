using API.DTOs;
using API.Helpers;

namespace API.Interfaces
{
  public interface IUserRepository
  {
    void Update(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser> GetUserByIdAsync(int id);
    Task<AppUser> GetUserByUsernameAsync(string username);
    Task<AppUser> GetUserByUsernameUnfilteredAsync(string username);
    //Optimized for member data gathering
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams); //Paging List
    Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser);
    Task<string> GetUserGender(string username);
    Task<AppUser> GetUserByPhotoId(int photoId);
  }
}