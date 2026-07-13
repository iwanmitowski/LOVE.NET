using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LoveNet.BuildingBlocks.Web.Validation;

public class AllowedFileExtensionsAttribute : ValidationAttribute
{
    private const string Message = "Unsupported file type";

    private readonly string[] extensions;

    public AllowedFileExtensionsAttribute(string[] extensions)
    {
        this.extensions = extensions;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var files = value switch
        {
            IFormFile single => new[] { single },
            IEnumerable<IFormFile?> many => many,
            _ => Enumerable.Empty<IFormFile?>(),
        };

        return files.Any(f => f is not null && !extensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))
            ? new ValidationResult(Message)
            : ValidationResult.Success;
    }
}
