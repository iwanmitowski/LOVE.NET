namespace LOVE.NET.Services.Images
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public interface IImagesService
    {
        Task<string> UploadImageAsync(IFormFile image);

        Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> images);
    }
}
