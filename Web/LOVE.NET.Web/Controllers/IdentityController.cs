namespace LOVE.NET.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Countries;
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
        private readonly ICountriesService countriesService;

        public IdentityController(
            IIdentityService userService,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ICountriesService countriesService)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.emailService = emailService;
            this.countriesService = countriesService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route(RegisterRoute)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string[]))]
        public async Task<IActionResult> RegisterAsync([FromForm] RegisterViewModel model)
        {
            await this.ValidateRegisterModelAsync(model);

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var result = await this.userService.RegisterAsync(model);

            if (result.Failure)
            {
                return this.BadRequest(result.Errors);
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

        [HttpPost]
        [Route(LogoutRoute)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = this.Request.Cookies[RefreshTokenValue];
            this.Response.Cookies.Delete(RefreshTokenValue);

            var user = await this.userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == refreshToken));

            var token = user?.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);

            if (token != null)
            {
                token.Revoked = DateTime.UtcNow;
                await this.userManager.UpdateAsync(user);
            }

            return this.Ok();
        }

        [HttpPost(RefreshTokenRoute)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = this.Request.Cookies[RefreshTokenValue];

            var user = await this.userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(x => x.RefreshTokens.Any(t => t.Token == refreshToken));

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

        [HttpGet(AccountRoute)]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsViewModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetAccount(string id)
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (loggedUserId != id)
            {
                return this.Forbid();
            }

            var user = this.userService.GetUserDetails(id);

            return this.Ok(user);
        }

        [HttpPut(AccountRoute)]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsViewModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult EditAccount(EditUserViewModel model)
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (loggedUserId != model.Id)
            {
                return this.Forbid();
            }

            var user = this.userService.GetUserDetails(model.Id);
            user.Bio = "changed";
            return this.Ok(user);
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

        private async Task ValidateRegisterModelAsync(RegisterViewModel model)
        {
            if (await this.userManager.Users.AnyAsync(x => x.Email == model.Email))
            {
                this.ModelState.AddModelError(Error, EmailAlreadyInUse);
            }

            this.BaseModelValidation(model);
        }

        private async Task ValidateEditModelAsync(EditUserViewModel model)
        {
            if (!await this.userManager.Users.AnyAsync(x => x.Email == model.Email))
            {
                this.ModelState.AddModelError(Error, InvalidEmail);
            }

            this.BaseModelValidation(model);
        }

        private void BaseModelValidation(RegisterViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                this.ModelState.AddModelError(Error, PasswordsDontMatch);
            }

            this.ValidateBirthday(model);

            if (model.GenderId < 1 || model.GenderId > GendersMaxCountInDb)
            {
                this.ModelState.AddModelError(Error, InvalidGender);
            }

            this.ValidateCountryAndCity(model);
        }

        private void ValidateCountryAndCity(RegisterViewModel model)
        {
            if (model.CityId < 1 || model.CityId > CitiesMaxCountInDb)
            {
                this.ModelState.AddModelError(Error, InvalidCity);
            }

            if (model.CountryId < 1 || model.CountryId > CountriesMaxCountInDb)
            {
                this.ModelState.AddModelError(Error, InvalidCountry);
            }

            var countryCities = this.countriesService.Get(model.CountryId);

            if (countryCities.Cities.Any(c => c.CityId == model.CityId) == false)
            {
                this.ModelState.AddModelError(Error, InvalidCity);
            }
        }

        private void ValidateBirthday(RegisterViewModel model)
        {
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
        }
    }
}
