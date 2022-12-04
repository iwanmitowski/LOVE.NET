namespace LOVE.NET.Web.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    using LOVE.NET.Services.Dashboard;
    using LOVE.NET.Web.ViewModels.Dashboard;
    using LOVE.NET.Web.ViewModels.Identity;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    using static LOVE.NET.Common.GlobalConstants;
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
        public async Task<IActionResult> GetStatisticsAsync()
        {
            var result = await this.dashboardService.GetStatisticsAsync();

            return this.Ok(result);
        }

        [HttpPost]
        [Route(UsersRoute)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsViewModel[]))]
        public async Task<IActionResult> GetUsers([FromBody] DashboardUserViewModel request)
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var result = await this.dashboardService.GetUsersAsync(request, loggedUserId);

            return this.Ok(result);
        }
    }
}
