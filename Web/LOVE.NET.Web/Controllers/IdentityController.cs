namespace LOVE.NET.Web.Controllers
{
    using System;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Identity;
    using LOVE.NET.Web.ViewModels.Identity;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;
    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;

    [Route(IdentityControllerName)]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService userService;
        private readonly UserManager<ApplicationUser> userManager;

        public IdentityController(
            IIdentityService userService,
            UserManager<ApplicationUser> userManager)
        {
            this.userService = userService;
            this.userManager = userManager;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(RegisterRoute)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string[]))]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterViewModel model)
        {
            await this.ValidateRegisterModel(model);

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.userService.RegisterAsync(model);

            if (result.Failure)
            {
                return this.BadRequest(result.Error);
            }

            // Email Confirmation
            var user = await this.userManager.FindByEmailAsync(model.Email);

            await this.SetRefreshToken(user);

            return this.StatusCode(201);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(LoginRoute)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result))]
        public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest((Result)FillAllTheInformation);
            }

            LoginResponseModel token;

            try
            {
                token = await this.userService.LoginAsync(model);
            }
            catch (Exception e)
            {
                return this.BadRequest((Result)e.Message);
            }

            this.Response.Cookies.Append(JWT, token.Token, new CookieOptions()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1),
            });

            var user = await this.userManager.FindByEmailAsync(model.Email);

            await this.SetRefreshToken(user);

            return this.Ok(token);
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return this.Ok("test");
        }

        private async Task SetRefreshToken(ApplicationUser user)
        {
            var refreshToken = this.userService.GenerateRefreshToken();

            user.RefreshTokens.Add(refreshToken);

            await this.userManager.UpdateAsync(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1.5),
            };

            this.Response.Cookies.Append(RefreshTokenValue, refreshToken.Token, cookieOptions);
        }

        private async Task ValidateRegisterModel(RegisterViewModel model)
        {
            if (await this.userManager.Users.AnyAsync(x => x.Email == model.Email))
            {
                this.ModelState.AddModelError(Error, EmailAlreadyInUse);
            }

            if (model.Password != model.ConfirmPassword)
            {
                this.ModelState.AddModelError(Error, PasswordsDontMatch);
            }

            var today = DateTime.UtcNow;
            var birthdate = model.Birthdate;

            var age = today.Year - birthdate.Year;

            // Leap years calculation
            if (birthdate > today.AddYears(-age))
            {
                age--;
            }

            if (age < MinimalAge)
            {
                this.ModelState.AddModelError(Error, UnderagedUser);
            }

            if (model.CityId < 1 || model.CityId > CitiesMaxCountInDb)
            {
                this.ModelState.AddModelError(Error, InvalidCity);
            }

            if (model.CountryId < 1 || model.CountryId > CountriesMaxCountInDb)
            {
                this.ModelState.AddModelError(Error, InvalidCountry);
            }
        }
    }
}
