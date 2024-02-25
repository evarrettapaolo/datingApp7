using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services
{
  public class PhotoServices : IPhotoService
  {
    private readonly Cloudinary _cloudinary;
    public PhotoServices(IOptions<CloudinarySettings> config)
    {
      //initialized with configured class instances from Program.cs
      var acc = new Account
      (
        config.Value.CloudName,
        config.Value.ApiKey,
        config.Value.ApiSecret
      );

      _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
      var uploadResult = new ImageUploadResult(); //object returned
      if(file.Length > 0)
      {
        using var stream = file.OpenReadStream(); //auto garbage disposal
        //params
        var uploadParams = new ImageUploadParams()
        {
          File = new FileDescription(file.FileName, stream),
          Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
          Folder = "da-net7"
        };
        uploadResult = await _cloudinary.UploadAsync(uploadParams);
      }

      return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
      var deleteParams = new DeletionParams(publicId);

      return await _cloudinary.DestroyAsync(deleteParams);
    }
  }
}