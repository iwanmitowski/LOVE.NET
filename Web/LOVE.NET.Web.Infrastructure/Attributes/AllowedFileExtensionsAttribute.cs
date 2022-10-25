namespace LOVE.NET.Web.Infrastructure.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;

    using Microsoft.AspNetCore.Http;

    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;

    public class AllowedFileExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] extensions;

        public AllowedFileExtensionsAttribute(string[] extensions)
        {
            this.extensions = extensions;
        }

        public string GetErrorMessage() => UnsupportedFileType;

        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            Type type = value.GetType();

            if (type.IsArray)
            {
                var files = value as IFormFile[] ?? Enumerable.Empty<IFormFile>();

                foreach (var file in files)
                {
                    if (file != null)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        if (!this.extensions.Contains(extension.ToLower()))
                        {
                            return new ValidationResult(this.GetErrorMessage());
                        }
                    }
                }
            }
            else
            {
                var file = value as IFormFile;

                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (!this.extensions.Contains(extension.ToLower()))
                    {
                        return new ValidationResult(this.GetErrorMessage());
                    }
                }
            }



            return ValidationResult.Success;
        }
    }
}
