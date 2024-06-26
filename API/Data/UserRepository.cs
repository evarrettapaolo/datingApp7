using API.DTOs;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class UserRepository : IUserRepository
  {
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    public UserRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
      _context = context;
    }

    public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
    {
      var query = _context.Users
        .Where(x => x.UserName == username)
        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider) //map entity to dto
        .AsQueryable();

      if (isCurrentUser) query = query.IgnoreQueryFilters();

      return await query.FirstOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
      //query object
      var query = _context.Users.AsQueryable();
      //build query, exclude logged in user and gender (if param is filled)
      query = query.Where(u => u.UserName != userParams.CurrentUsername);
      query = query.Where(u => u.Gender == userParams.Gender);
      //earliest and latest birth date 
      var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
      var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));
      //age params filtering
      query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
      //sorting
      query = userParams.OrderBy switch
      {
        "created" => query.OrderByDescending(u => u.Created),
        _ => query.OrderByDescending(u => u.LastActive)
      };
      //utilize the static method to execute paging
      return await PagedList<MemberDto>.CreateAsync(
          query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider),
          userParams.PageNumber,
          userParams.PageSize
        );
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
      return await _context.Users.FindAsync(id);
    }

    public Task<AppUser> GetUserByPhotoId(int photoId)
    {
      return _context.Users
        .Include(p => p.Photos)
        .IgnoreQueryFilters()
        .Where(p => p.Photos.Any(p => p.Id == photoId))
        .FirstOrDefaultAsync();
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
      return await _context.Users
        .Include(p => p.Photos) //load the photos array
        .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<AppUser> GetUserByUsernameUnfilteredAsync(string username)
    {
      return await _context.Users
        .IgnoreQueryFilters()
        .Include(p => p.Photos)
        .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<string> GetUserGender(string username)
    {
      return await _context.Users.Where(x => x.UserName == username)
        .Select(x => x.Gender)
        .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
      return await _context.Users
        .Include(p => p.Photos)
        .ToListAsync();
    }

    public void Update(AppUser user)
    {
      _context.Entry(user).State = EntityState.Modified; //notify EF tracker for changes, not saved yet
    }
  }
}