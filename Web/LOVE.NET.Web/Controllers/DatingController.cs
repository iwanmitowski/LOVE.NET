namespace LOVE.NET.Web.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Dating;
    using LOVE.NET.Services.Identity;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;

    [Route(DatingControllerName)]
    [ApiController]
    public class DatingController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IDatingService datingService;

        public DatingController(
            UserManager<ApplicationUser> userManager,
            IDatingService datingService)
        {
            this.userManager = userManager;
            this.datingService = datingService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetNotSwipedUsersAsync()
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = await this.userManager.FindByIdAsync(loggedUserId);

            if (user == null)
            {
                return this.NotFound();
            }

            var notSwipedUsers = this.datingService.GetNotSwipedUsersAsync(loggedUserId);

            return this.Ok(notSwipedUsers);
        }
    }
}
