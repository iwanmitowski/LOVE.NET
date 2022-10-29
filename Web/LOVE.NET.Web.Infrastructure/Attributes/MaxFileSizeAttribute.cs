namespace LOVE.NET.Web.Infrastructure.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Microsoft.AspNetCore.Http;

    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;

    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            this.maxFileSize = maxFileSize;
        }

        public string GetErrorMessage() => MaxFileSizeReached;

        protected override ValidationResult IsValid(
            object value,
            ValidationContext validationContext)
        {
            Type type = value.GetType();

            if (type.IsArray)
            {
                var files = value as IFormFile[] ?? Enumerable.Empty<IFormFile>();

                foreach (var file in files)
                {
                    if (file != null && file.Length > this.maxFileSize)
                    {
                        return new ValidationResult(this.GetErrorMessage());
                    }
                }
            }
            else
            {
                var file = value as IFormFile;

                if (file != null && file.Length > this.maxFileSize)
                {
                    return new ValidationResult(this.GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }
    }
}
