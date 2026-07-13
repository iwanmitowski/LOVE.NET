using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace LoveNet.BuildingBlocks.Media;

/// <summary>Cloudinary-backed image storage, ported from the monolith's Images/ImagesService.</summary>
public class ImagesService : IImagesService
{
    private readonly Cloudinary cloudinary;

    public ImagesService(Cloudinary cloudinary)
    {
        this.cloudinary = cloudinary;
    }

    public async Task<string> UploadImageAsync(IFormFile image)
    {
        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(Guid.NewGuid().ToString(), memoryStream),
        };

        var result = await cloudinary.UploadAsync(uploadParams);

        return result.SecureUrl.AbsoluteUri;
    }

    public async Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> images)
    {
        var uploads = images
            .Where(image => image is not null)
            .Select(UploadImageAsync)
            .ToArray();

        return await Task.WhenAll(uploads);
    }
}
