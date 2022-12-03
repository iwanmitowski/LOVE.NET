namespace LOVE.NET.Services.Dashboard
{
    using System.Threading.Tasks;

    using LOVE.NET.Web.ViewModels.Dashboard;

    public interface IDashboardService
    {
        public Task<StatisticsViewModel> GetStatisticsAsync();
    }
}
