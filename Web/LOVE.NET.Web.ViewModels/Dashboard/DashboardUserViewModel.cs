namespace LOVE.NET.Web.ViewModels.Dashboard
{
    using System.Collections.Generic;

    using LOVE.NET.Web.ViewModels.Identity;

    public class DashboardUserViewModel
    {
        public IEnumerable<UserDetailsViewModel> Users { get; set; }

        public int TotalUsers { get; set; }
    }
}
