using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoveNet.BuildingBlocks.Media;

public static class MediaExtensions
{
    /// <summary>
    /// Registers <see cref="IImagesService"/> (config section "Cloudinary").
    /// The Cloudinary client is deferred behind a Lazy so hosts start — and
    /// image-less flows run — without credentials; only an actual upload
    /// attempt fails loudly when the section is empty.
    /// </summary>
    public static IServiceCollection AddCloudinaryMedia(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => new Lazy<Cloudinary>(() =>
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:Key"],
                configuration["Cloudinary:Secret"]);

            var cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;

            return cloudinary;
        }));

        services.AddSingleton<IImagesService, ImagesService>();

        return services;
    }
}
