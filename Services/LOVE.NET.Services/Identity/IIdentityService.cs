namespace LOVE.NET.Services.Identity
{
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Web.ViewModels.Identity;

    public interface IIdentityService
    {
        Task<string> GenerateJwtToken(ApplicationUser user);

        Task<LoginResponseModel> LoginAsync(LoginViewModel model);

        Task<Result> RegisterAsync(RegisterViewModel model);

        RefreshToken GenerateRefreshToken();

        UserDetailsViewModel GetUserDetails(string id);

        Task EditUserAsync(EditUserViewModel model);
    }
}
