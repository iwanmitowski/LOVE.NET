namespace LOVE.NET.Web.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Dating;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
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
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNotSwipedUsersAsync()
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = await this.userManager.FindByIdAsync(loggedUserId);

            if (user == null)
            {
                return this.Unauthorized();
            }

            var notSwipedUsers = this.datingService.GetNotSwipedUsers(loggedUserId);

            return this.Ok(notSwipedUsers);
        }

        [HttpPost]
        [Authorize]
        [Route(LikeRoute)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LikeAsync(string likedUserId)
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var result = await this.datingService.LikeAsync(loggedUserId, likedUserId);

            if (result.Failure)
            {
                return this.BadRequest((Result)string.Join('\n', result.Errors));
            }

            return this.Ok();
        }
    }
}
