namespace LOVE.NET.Web.Controllers
{
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Identity;
    using LOVE.NET.Web.ViewModels.Identity;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;
    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;

    [Route("api/[controller]")]
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

            // Set Refresh Token
            return this.StatusCode(201);
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return this.Ok("test");
        }

        private async Task ValidateRegisterModel(RegisterViewModel model)
        {
            if (await this.userManager.Users.AnyAsync(x => x.Email == model.Email))
            {
                this.ModelState.AddModelError("Error", EmailAlreadyInUse);
            }

            if (model.Password != model.ConfirmPassword)
            {
                this.ModelState.AddModelError("Error", PasswordsDontMatch);
            }

            // TODO: Validate country and city
        }
    }
}
