using System.Collections.Generic;

using LOVE.NET.Web.ViewModels.Identity;

namespace LOVE.NET.Web.ViewModels.Dashboard
{
    public class DashboardUserViewModel
    {
        public IEnumerable<UserDetailsViewModel> Users { get; set; }

        public int TotalUsers { get; set; }
    }
}
