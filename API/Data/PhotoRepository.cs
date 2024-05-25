using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class PhotoRepository : IPhotoRepository
  {
    private readonly DataContext _context;
    public PhotoRepository(DataContext context)
    {
      _context = context;
    }

    public async Task<Photo> GetPhotoById(int photoId)
    {
      return await _context.Photos
        .IgnoreQueryFilters()
        .SingleOrDefaultAsync(x => x.Id == photoId);
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
    {
      return await _context.Photos
        .IgnoreQueryFilters()
        .Where(x => x.IsApproved == false)
        .Select(u => new PhotoForApprovalDto
        {
          Id = u.Id,
          Username = u.AppUser.UserName,
          Url = u.Url,
          IsApproved = u.IsApproved
        })
        .ToListAsync();
    }

    public void RemovePhoto(Photo photo)
    {
      _context.Photos.Remove(photo);
    }
  }
}