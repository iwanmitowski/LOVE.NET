namespace LOVE.NET.Web.Controllers
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Services.Dashboard;
    using LOVE.NET.Web.ViewModels.Dashboard;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;
    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;

    [Authorize(Roles = AdministratorRoleName)]
    [Route(DashboardControllerName)]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            this.dashboardService = dashboardService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StatisticsViewModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetStatistics()
        {
            var result = this.dashboardService.GetStatistics();

            return this.Ok(result);
        }

        [HttpPost]
        [Route(UsersRoute)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardUserViewModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers([FromBody] DashboardUserRequestViewModel request)
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var result = await this.dashboardService.GetUsersAsync(request, loggedUserId);

            return this.Ok(result);
        }

        [HttpPost]
        [Route(ModerateRoute)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ModerateUserAsync([FromBody] ModerateUserViewModel request)
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (loggedUserId == request.UserId)
            {
                return this.BadRequest(CantBanYourself);
            }

            if (request.BannedUntil.HasValue && request.BannedUntil.Value.Date < DateTime.UtcNow.Date)
            {
                return this.BadRequest(CantBanUserInThePast);
            }

            var serviceResult = await this.dashboardService.ModerateAsync(request);

            if (serviceResult.Errors != null)
            {
                return this.BadRequest((Result)string.Join('\n', serviceResult.Errors));
            }

            return this.NoContent();
        }
    }
}
