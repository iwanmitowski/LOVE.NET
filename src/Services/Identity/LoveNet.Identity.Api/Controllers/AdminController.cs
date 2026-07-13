using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.BuildingBlocks.Web.Middleware;
using LoveNet.Identity.Api.Models;
using LoveNet.Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoveNet.Identity.Api.Controllers;

[Authorize(Roles = LoveNetConstants.AdministratorRoleName)]
[Route("api/admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IIdentityService identityService;

    public AdminController(IIdentityService identityService)
    {
        this.identityService = identityService;
    }

    [HttpPost("moderate")]
    public async Task<IActionResult> ModerateUserAsync([FromBody] ModerateUserModel request)
    {
        if (User.GetUserId() == request.UserId)
        {
            return BadRequest(IdentityMessages.CantBanYourself);
        }

        if (request.BannedUntil.HasValue && request.BannedUntil.Value.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(IdentityMessages.CantBanUserInThePast);
        }

        var result = await identityService.ModerateAsync(request, HttpContext.GetCorrelationId());

        if (result.Failure)
        {
            return BadRequest((Result)string.Join('\n', result.Errors));
        }

        return NoContent();
    }
}
