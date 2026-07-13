using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoveNet.BuildingBlocks.Media;

public static class MediaExtensions
{
    /// <summary>Registers the Cloudinary client + <see cref="IImagesService"/> (config section "Cloudinary").</summary>
    public static IServiceCollection AddCloudinaryMedia(this IServiceCollection services, IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:Key"],
            configuration["Cloudinary:Secret"]);

        var cloudinary = new Cloudinary(account);
        cloudinary.Api.Secure = true;

        services.AddSingleton(cloudinary);
        services.AddSingleton<IImagesService, ImagesService>();

        return services;
    }
}
