namespace LOVE.NET.Web.ViewModels.Dating
{
    using System.Collections.Generic;

    using LOVE.NET.Web.ViewModels.Identity;

    public class MatchesViewModel
    {
        public IEnumerable<UserMatchViewModel> Matches { get; set; }

        public int TotalMatches { get; set; }
    }
}
