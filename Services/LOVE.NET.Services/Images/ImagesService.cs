namespace LOVE.NET.Services.Images
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;

    using Microsoft.AspNetCore.Http;

    using static LOVE.NET.Common.GlobalConstants;

    public class ImagesService : IImagesService
    {
        private readonly Cloudinary cloudinary;

        public ImagesService(Cloudinary cloudinary)
        {
            this.cloudinary = cloudinary;
        }

        public async Task<string> UploadImageAsync(IFormFile image)
        {
            byte[] imageBytes;
            string imageUrl;

            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            using (var memoryStream = new MemoryStream(imageBytes))
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(
                        Guid.NewGuid().ToString(),
                        memoryStream),
                };

                var result = await this.cloudinary.UploadAsync(uploadParams);

                imageUrl = result.SecureUrl.AbsoluteUri;
            }

            return imageUrl;
        }

        public async Task<IEnumerable<string>> UploadImagesAsync(IEnumerable<IFormFile> images)
        {
            var imagesArray = images.ToArray();
            var imagesCount = imagesArray.Length; 
            var imageTasks = new Task<string>[imagesCount];

            for (int i = 0; i < imagesCount; i++)
            {
                if (imagesArray[i] != null)
                {
                    imageTasks[i] = this.UploadImageAsync(imagesArray[i]);
                }
            }

            await Task.WhenAll(imageTasks);

            var urls = imageTasks.Select(x => x.Result)?.ToList() ?? Enumerable.Empty<string>();

            return urls;
        }
    }
}
