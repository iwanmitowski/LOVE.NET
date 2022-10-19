namespace LOVE.NET.Services.Email
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Messaging;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.WebUtilities;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;
    using static LOVE.NET.Common.GlobalConstants.EmailMessagesConstants;
    using static LOVE.NET.Common.GlobalConstants.EmailSenderConstants;

    public class EmailService : IEmailService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly IWebHostEnvironment environment; // Add in .csproj Microsoft.AspNetCore.App

        public EmailService(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IWebHostEnvironment environment)
        {
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.environment = environment;
        }

        public async Task SendEmailConfirmationAsync(string origin, ApplicationUser user)
        {
            var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var verifyUrl = string.Format(
                VerifyUrl,
                origin,
                VerifyEmailRoute,
                token,
                user.Email);

            var path = Path.Combine(
                this.environment.WebRootPath,
                Templates,
                Email,
                VerifyHtml);

            var subject = File.ReadAllText(path);

            var content = string.Format(subject, verifyUrl);

            await this.emailSender.SendEmailAsync(
                FromEmail,
                FromName,
                user.Email,
                EmailSubject,
                content);
        }

        public async Task<Result> VerifyEmailAsync(string email, string token)
        {
            var user = await this.userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Unauthorized;
            }

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);

            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await this.userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                return IncorrectEmail;
            }

            user.LockoutEnabled = false;
            await this.userManager.UpdateAsync(user);

            return true;
        }

        public async Task<Result> ResendEmailConfirmationLinkAsync(string email, string origin)
        {
            var user = await this.userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return EmailDoesntMatch;
            }

            if (!user.LockoutEnabled)
            {
                return EmailAlreadyVerified;
            }

            await this.SendEmailConfirmationAsync(origin, user);

            return true;
        }
    }
}
