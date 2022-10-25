namespace LOVE.NET.Services.Images
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public interface IImagesService
    {
        public Task<string> UploadImageAsync(IFormFile image);

        public Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> images);
    }
}
