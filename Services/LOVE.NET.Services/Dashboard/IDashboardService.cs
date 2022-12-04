namespace LOVE.NET.Services.Dashboard
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LOVE.NET.Web.ViewModels.Dashboard;
    using LOVE.NET.Web.ViewModels.Identity;

    public interface IDashboardService
    {
        public Task<StatisticsViewModel> GetStatisticsAsync();

        Task<IEnumerable<UserDetailsViewModel>> GetUsersAsync(
            DashboardUserViewModel request,
            string loggedUserId);
    }
}
