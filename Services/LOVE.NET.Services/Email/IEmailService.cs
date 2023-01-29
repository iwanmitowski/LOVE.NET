namespace LOVE.NET.Services.Email
{
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Web.ViewModels.Identity;

    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string origin, ApplicationUser user);

        Task<Result> VerifyEmailAsync(string email, string token);

        Task<Result> ResetPasswordAsync(ResetPasswordViewModel model);

        Task<Result> ResendEmailConfirmationLinkAsync(string email, string origin);

        Task<Result> SendResetPasswordLinkAsync(string email, string origin);
    }
}
