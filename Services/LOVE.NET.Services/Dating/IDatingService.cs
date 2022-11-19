namespace LOVE.NET.Services.Dating
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LOVE.NET.Web.ViewModels.Identity;

    public interface IDatingService
    {
        public Task<IEnumerable<UserMatchViewModel>> GetNotSwipedUsersAsync(string userId);
    }
}
