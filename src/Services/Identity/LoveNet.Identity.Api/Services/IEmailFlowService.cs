using LoveNet.BuildingBlocks.Web;
using LoveNet.Identity.Api.Data;
using LoveNet.Identity.Api.Models;

namespace LoveNet.Identity.Api.Services;

/// <summary>
/// Owns UserManager token generation/validation for the email flows. Actual
/// delivery happens in the Notifications service via email-requested events.
/// </summary>
public interface IEmailFlowService
{
    Task SendEmailConfirmationAsync(string? origin, ApplicationUser user, string? correlationId);

    Task<Result> SendResetPasswordLinkAsync(string email, string? origin, string? correlationId);

    Task<Result> ResendEmailConfirmationLinkAsync(string email, string? origin, string? correlationId);

    Task<Result> VerifyEmailAsync(string email, string token);

    Task<Result> ResetPasswordAsync(ResetPasswordModel model);
}
