﻿namespace LOVE.NET.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Email;
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
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IIdentityService userService;
        private readonly IEmailService emailService;

        public IdentityController(
            IIdentityService userService,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.emailService = emailService;
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

            await this.EmailConfirmation(model.Email);

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

            var user = await this.userManager.FindByEmailAsync(model.Email);

            await this.SetRefreshToken(user);

            return this.Ok(token);
        }

        [Authorize]
        [HttpPost(RefreshTokenRoute)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = this.Request.Cookies[RefreshTokenValue];

            var user = await this.userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(x => x.Email == this.User.FindFirstValue(ClaimTypes.Email));

            if (user == null)
            {
                return this.Unauthorized();
            }

            var oldToken = user.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);

            if (oldToken != null && !oldToken.IsActive)
            {
                return this.Unauthorized();
            }

            var userRoles = await this.userManager.GetRolesAsync(user);
            var newToken = await this.userService.GenerateJwtToken(user);

            await this.SetRefreshToken(user, oldToken);

            var response = new LoginResponseModel()
            {
                Id = user.Id,
                Token = newToken,
                Email = user.Email,
                UserName = user.UserName,
                IsAdmin = userRoles.Any(r => r == AdministratorRoleName),
            };

            return this.Ok(response);
        }

        [Authorize]
        [HttpGet("test")]
        public IActionResult Test()
        {
            return this.Ok("test");
        }

        private async Task SetRefreshToken(
            ApplicationUser user,
            RefreshToken oldRefreshToken = null)
        {
            var refreshToken = this.userService.GenerateRefreshToken();

            if (oldRefreshToken != null)
            {
                oldRefreshToken.Revoked = DateTime.UtcNow;
                oldRefreshToken.ReplacedByToken = refreshToken.Token;
            }

            user.RefreshTokens.Add(refreshToken);

            await this.userManager.UpdateAsync(user);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(1.5),
                SameSite = SameSiteMode.None,
                Secure = true,
            };

            this.Response.Cookies.Append(RefreshTokenValue, refreshToken.Token, cookieOptions);
        }

        private async Task EmailConfirmation(string email)
        {
            var user = await this.userManager.FindByEmailAsync(email);

            var origin = this.Request.Headers[HeaderOrigin];

            await this.emailService.SendEmailConfirmationAsync(origin, user);
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
