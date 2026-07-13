using System.Text;
using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Web;
using LoveNet.Identity.Api.Data;
using LoveNet.Identity.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace LoveNet.Identity.Api.Services;

public class EmailFlowService : IEmailFlowService
{
    // {0}=origin {1}=route {2}=token {3}=email — same link format the SPA parses.
    private const string LinkFormat = "{0}/{1}?token={2}&email={3}";
    private const string VerifyRoute = "verify";
    private const string ResetPasswordRoute = "resetPassword";

    private readonly UserManager<ApplicationUser> userManager;
    private readonly AppIdentityDbContext db;

    public EmailFlowService(UserManager<ApplicationUser> userManager, AppIdentityDbContext db)
    {
        this.userManager = userManager;
        this.db = db;
    }

    public async Task SendEmailConfirmationAsync(string? origin, ApplicationUser user, string? correlationId)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var link = string.Format(LinkFormat, origin, VerifyRoute, token, user.Email);

        db.AddOutboxMessage(
            KafkaTopics.EmailRequested,
            user.Id,
            new EmailRequestedEvent(EmailType.ConfirmEmail, user.Email!, user.UserName!, link),
            correlationId);
        await db.SaveChangesAsync();
    }

    public async Task<Result> SendResetPasswordLinkAsync(string email, string? origin, string? correlationId)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return IdentityMessages.EmailDoesntMatch;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var link = string.Format(LinkFormat, origin, ResetPasswordRoute, token, user.Email);

        db.AddOutboxMessage(
            KafkaTopics.EmailRequested,
            user.Id,
            new EmailRequestedEvent(EmailType.ResetPassword, user.Email!, user.UserName!, link),
            correlationId);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<Result> ResendEmailConfirmationLinkAsync(string email, string? origin, string? correlationId)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return IdentityMessages.EmailDoesntMatch;
        }

        // Quirk ported from the monolith: LockoutEnabled doubles as the
        // "email verified" marker (set by VerifyEmailAsync below).
        if (user.LockoutEnabled)
        {
            return IdentityMessages.EmailAlreadyVerified;
        }

        await SendEmailConfirmationAsync(origin, user, correlationId);

        return true;
    }

    public async Task<Result> VerifyEmailAsync(string email, string token)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return "Unauthorized";
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            return IdentityMessages.IncorrectEmail;
        }

        user.LockoutEnabled = true;
        await userManager.UpdateAsync(user);

        return true;
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);

        if (user is null)
        {
            return "Unauthorized";
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        var result = await userManager.ResetPasswordAsync(user, decodedToken, model.Password);

        if (!result.Succeeded)
        {
            return string.Join("\n", result.Errors.Select(x => x.Description));
        }

        return true;
    }
}
