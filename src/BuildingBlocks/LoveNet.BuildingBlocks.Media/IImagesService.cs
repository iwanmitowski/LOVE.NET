using Microsoft.AspNetCore.Http;

namespace LoveNet.BuildingBlocks.Media;

public interface IImagesService
{
    Task<string> UploadImageAsync(IFormFile image);

    Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> images);
}
