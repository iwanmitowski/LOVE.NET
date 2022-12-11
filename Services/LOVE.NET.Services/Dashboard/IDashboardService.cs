namespace LOVE.NET.Services.Dashboard
{
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Web.ViewModels.Dashboard;

    public interface IDashboardService
    {
        StatisticsViewModel GetStatistics();

        Task<DashboardUserViewModel> GetUsersAsync(
            DashboardUserRequestViewModel request,
            string loggedUserId);

        Task<Result> ModerateAsync(ModerateUserViewModel request);
    }
}
