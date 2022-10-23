﻿namespace LOVE.NET.Services.Images
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using Microsoft.AspNetCore.Http;

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
    }
}
