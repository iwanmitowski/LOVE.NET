using LoveNet.BuildingBlocks.Web;
using LoveNet.BuildingBlocks.Web.Middleware;
using LoveNet.Identity.Api.Data;
using LoveNet.Identity.Api.Models;
using LoveNet.Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Identity.Api.Controllers;

[Route("api/identity")]
[ApiController]
public class IdentityController : ControllerBase
{
    private const string RefreshTokenCookie = "refreshToken";

    private readonly UserManager<ApplicationUser> userManager;
    private readonly IIdentityService identityService;
    private readonly IEmailFlowService emailFlowService;

    public IdentityController(
        UserManager<ApplicationUser> userManager,
        IIdentityService identityService,
        IEmailFlowService emailFlowService)
    {
        this.userManager = userManager;
        this.identityService = identityService;
        this.emailFlowService = emailFlowService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync([FromForm] RegisterModel model)
    {
        await ValidateRegisterModelAsync(model);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await identityService.RegisterAsync(model, HttpContext.GetCorrelationId());

        if (!result.Succeeded)
        {
            var errorMessages = string.Join("\n", result.Errors.Select(x => x.Description));
            return BadRequest((Result)errorMessages);
        }

        var user = await userManager.FindByEmailAsync(model.Email);

        await emailFlowService.SendEmailConfirmationAsync(
            Request.Headers.Origin,
            user!,
            HttpContext.GetCorrelationId());

        await SetRefreshToken(user!);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest((Result)IdentityMessages.FillAllTheInformation);
        }

        LoginResponseModel response;

        try
        {
            response = await identityService.LoginAsync(model);
        }
        catch (ArgumentException e)
        {
            return BadRequest((Result)e.Message);
        }

        var user = await userManager.FindByEmailAsync(model.Email);

        await SetRefreshToken(user!);

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];
        Response.Cookies.Delete(RefreshTokenCookie);

        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == refreshToken));

        var token = user?.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);

        if (token != null)
        {
            token.Revoked = DateTime.UtcNow;
            await userManager.UpdateAsync(user!);
        }

        return Ok();
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookie];

        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == refreshToken));

        if (user is null)
        {
            return Unauthorized();
        }

        var oldToken = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);

        if (oldToken != null && !oldToken.IsActive)
        {
            return Unauthorized();
        }

        var userRoles = await userManager.GetRolesAsync(user);
        var newToken = await identityService.GenerateJwtToken(user);

        await SetRefreshToken(user, oldToken);

        var response = new LoginResponseModel
        {
            Id = user.Id,
            Token = newToken,
            Email = user.Email!,
            UserName = user.UserName!,
            IsAdmin = userRoles.Contains(LoveNetConstants.AdministratorRoleName),
        };

        return Ok(response);
    }

    private async Task SetRefreshToken(ApplicationUser user, RefreshToken? oldRefreshToken = null)
    {
        var refreshToken = identityService.GenerateRefreshToken();

        if (oldRefreshToken != null)
        {
            oldRefreshToken.Revoked = DateTime.UtcNow;
            oldRefreshToken.ReplacedByToken = refreshToken.Token;
        }

        user.RefreshTokens.Add(refreshToken);

        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddHours(1.5),
            SameSite = SameSiteMode.None,
            Secure = true,
        };

        Response.Cookies.Append(RefreshTokenCookie, refreshToken.Token, cookieOptions);
    }

    private async Task ValidateRegisterModelAsync(RegisterModel model)
    {
        if (await userManager.Users.AnyAsync(x => x.Email == model.Email))
        {
            ModelState.AddModelError("Error", IdentityMessages.EmailAlreadyInUse);
        }

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("Error", IdentityMessages.PasswordsDontMatch);
        }

        if (DateHelper.CalculateAge(model.Birthdate) < LoveNetConstants.MinimalAge)
        {
            ModelState.AddModelError("Error", IdentityMessages.UnderagedUser);
        }

        // Range checks only — the Profile service owns the reference data and
        // is the authority on geo consistency.
        if (model.GenderId < 1 || model.GenderId > IdentityMessages.GendersMaxCountInDb)
        {
            ModelState.AddModelError("Error", IdentityMessages.InvalidGender);
        }

        if (model.CityId < 1 || model.CityId > IdentityMessages.CitiesMaxCountInDb)
        {
            ModelState.AddModelError("Error", IdentityMessages.InvalidCity);
        }

        if (model.CountryId < 1 || model.CountryId > IdentityMessages.CountriesMaxCountInDb)
        {
            ModelState.AddModelError("Error", IdentityMessages.InvalidCountry);
        }
    }
}
