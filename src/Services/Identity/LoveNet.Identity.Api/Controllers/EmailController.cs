using LoveNet.BuildingBlocks.Web.Middleware;
using LoveNet.Identity.Api.Models;
using LoveNet.Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoveNet.Identity.Api.Controllers;

[Route("api/email")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly IEmailFlowService emailFlowService;

    public EmailController(IEmailFlowService emailFlowService)
    {
        this.emailFlowService = emailFlowService;
    }

    [HttpGet("sendResetPasswordLink")]
    [AllowAnonymous]
    public async Task<IActionResult> SendResetPasswordLinkAsync(string email)
    {
        var result = await emailFlowService.SendResetPasswordLinkAsync(
            email, Request.Headers.Origin, HttpContext.GetCorrelationId());

        if (result.Failure)
        {
            return BadRequest(result.Errors);
        }

        return Ok(IdentityMessages.CheckYourEmail);
    }

    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPasswordAsync(ResetPasswordModel model)
    {
        var result = await emailFlowService.ResetPasswordAsync(model);

        if (result.Failure)
        {
            return BadRequest(result.Errors);
        }

        return Ok(IdentityMessages.PasswordResetSuccessful);
    }

    [HttpGet("verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
    {
        var result = await emailFlowService.VerifyEmailAsync(email, token);

        if (result.Failure)
        {
            return BadRequest(result.Errors);
        }

        return Ok(IdentityMessages.EmailConfirmed);
    }

    [HttpGet("resendEmailConfirmationLink")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailConfirmationLink(string email)
    {
        var result = await emailFlowService.ResendEmailConfirmationLinkAsync(
            email, Request.Headers.Origin, HttpContext.GetCorrelationId());

        if (result.Failure)
        {
            return BadRequest(result.Errors);
        }

        return Ok(IdentityMessages.CheckYourEmail);
    }
}
