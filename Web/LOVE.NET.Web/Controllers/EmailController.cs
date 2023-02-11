namespace LOVE.NET.Web.Controllers
{
    using System.Threading.Tasks;

    using LOVE.NET.Services.Email;
    using LOVE.NET.Web.ViewModels.Identity;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;
    using static LOVE.NET.Common.GlobalConstants.EmailMessagesConstants;

    [Route(EmailControllerName)]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService emailService;

        public EmailController(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(SendResetPasswordLinkLinkRoute)]
        public async Task<IActionResult> SendResetPasswordLinkAsync(string email)
        {
            var origin = this.Request.Headers[HeaderOrigin];

            var result = await this.emailService.SendResetPasswordLinkAsync(email, origin);

            if (result.Failure)
            {
                return this.BadRequest(result.Errors);
            }

            return this.Ok(CheckYourEmail);
        }

        [HttpPost]
        [Route(ResetPasswordRoute)]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var result = await this.emailService.ResetPasswordAsync(model);

            if (result.Failure)
            {
                return this.BadRequest(result.Errors);
            }

            return this.Ok(PasswordResetSuccessful);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(VerifyEmailRoute)]
        public async Task<IActionResult> VerifyEmail(
            [FromQuery] string email,
            [FromQuery] string token)
        {
            var result = await this.emailService.VerifyEmailAsync(email, token);

            if (result.Failure)
            {
                return this.BadRequest(result.Errors);
            }

            return this.Ok(EmailConfirmed);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route(ResendEmailConfirmationLinkRoute)]
        public async Task<IActionResult> ResendEmailConfirmationLink(string email)
        {
            var origin = this.Request.Headers[HeaderOrigin];

            var result = await this.emailService.ResendEmailConfirmationLinkAsync(email, origin);

            if (result.Failure)
            {
                return this.BadRequest(result.Errors);
            }

            return this.Ok(CheckYourEmail);
        }
    }
}
