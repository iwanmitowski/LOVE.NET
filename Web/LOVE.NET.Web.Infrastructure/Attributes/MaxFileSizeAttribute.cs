namespace LOVE.NET.Web.Infrastructure.Attributes
{
    using System.ComponentModel.DataAnnotations;

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
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > this.maxFileSize)
                {
                    return new ValidationResult(this.GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }
    }
}
