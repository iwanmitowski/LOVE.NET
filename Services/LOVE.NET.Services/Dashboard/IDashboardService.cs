namespace LOVE.NET.Services.Dashboard
{
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Web.ViewModels.Dashboard;

    public interface IDashboardService
    {
        public Task<StatisticsViewModel> GetStatisticsAsync();

        Task<DashboardUserViewModel> GetUsersAsync(
            DashboardUserRequestViewModel request,
            string loggedUserId);

        public Task<Result> ModerateAsync(ModerateUserViewModel request);
    }
}
