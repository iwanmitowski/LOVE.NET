using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LoveNet.BuildingBlocks.Web.Validation;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private const string Message = "Max file size reached";

    private readonly int maxFileSize;

    public MaxFileSizeAttribute(int maxFileSize)
    {
        this.maxFileSize = maxFileSize;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var files = value switch
        {
            IFormFile single => new[] { single },
            IEnumerable<IFormFile?> many => many,
            _ => Enumerable.Empty<IFormFile?>(),
        };

        return files.Any(f => f is not null && f.Length > maxFileSize)
            ? new ValidationResult(Message)
            : ValidationResult.Success;
    }
}
