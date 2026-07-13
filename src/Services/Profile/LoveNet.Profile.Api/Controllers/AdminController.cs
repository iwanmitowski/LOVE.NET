using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.Profile.Api.Models;
using LoveNet.Profile.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoveNet.Profile.Api.Controllers;

[Authorize(Roles = LoveNetConstants.AdministratorRoleName)]
[Route("api/admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IProfileService profileService;

    public AdminController(IProfileService profileService)
    {
        this.profileService = profileService;
    }

    [HttpPost("users")]
    public async Task<IActionResult> GetUsers([FromBody] AdminUsersRequestModel request)
    {
        var result = await profileService.GetUsersForAdminAsync(request, User.GetUserId());

        return Ok(result);
    }
}
