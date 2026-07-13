using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.BuildingBlocks.Web.Middleware;
using LoveNet.Profile.Api.Models;
using LoveNet.Profile.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoveNet.Profile.Api.Controllers;

[Route("api/profile")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IProfileService profileService;

    public ProfileController(IProfileService profileService)
    {
        this.profileService = profileService;
    }

    [HttpGet("account/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetAccount(string id)
    {
        if (User.GetUserId() != id)
        {
            return Forbid();
        }

        var user = await profileService.GetUserDetailsAsync(id);

        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("account/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> EditAccount([FromForm] EditUserModel model)
    {
        if (User.GetUserId() != model.Id)
        {
            return Forbid();
        }

        await ValidateEditModelAsync(model);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await profileService.EditUserAsync(model, HttpContext.GetCorrelationId());
        var user = await profileService.GetUserDetailsAsync(model.Id);

        return Ok(user);
    }

    private async Task ValidateEditModelAsync(EditUserModel model)
    {
        if (DateHelper.CalculateAge(model.Birthdate) < LoveNetConstants.MinimalAge)
        {
            ModelState.AddModelError("Error", "You need to be 18 years old to register");
        }

        if (!await profileService.CityBelongsToCountryAsync(model.CityId, model.CountryId))
        {
            ModelState.AddModelError("Error", "Invalid city");
        }
    }
}
