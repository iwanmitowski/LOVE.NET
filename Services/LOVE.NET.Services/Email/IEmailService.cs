namespace LOVE.NET.Services.Email
{
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;

    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string origin, ApplicationUser user);

        Task<Result> VerifyEmailAsync(string email, string token);

        Task<Result> ResendEmailConfirmationLinkAsync(string email, string origin);
    }
}
