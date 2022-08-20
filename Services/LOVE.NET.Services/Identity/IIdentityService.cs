namespace LOVE.NET.Services.Identity
{
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Web.ViewModels.Identity;

    public interface IIdentityService
    {
        Task<Result> RegisterAsync(RegisterViewModel model);
    }
}
