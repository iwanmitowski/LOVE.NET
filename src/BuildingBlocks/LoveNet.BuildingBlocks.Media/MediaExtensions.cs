using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoveNet.BuildingBlocks.Media;

public static class MediaExtensions
{
    /// <summary>
    /// Registers the Cloudinary client + <see cref="IImagesService"/> (config
    /// section "Cloudinary"). Construction is deferred so hosts start (and
    /// design-time tools run) without credentials; uploads then fail loudly.
    /// </summary>
    public static IServiceCollection AddCloudinaryMedia(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ =>
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:Key"],
                configuration["Cloudinary:Secret"]);

            var cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;

            return cloudinary;
        });

        services.AddSingleton<IImagesService, ImagesService>();

        return services;
    }
}
