namespace LOVE.NET.Services.Images
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public interface IImagesService
    {
        public Task<string> UploadImageAsync(IFormFile image);
    }
}
