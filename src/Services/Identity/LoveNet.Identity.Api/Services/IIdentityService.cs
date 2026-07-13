using LoveNet.BuildingBlocks.Web;
using LoveNet.Identity.Api.Data;
using LoveNet.Identity.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace LoveNet.Identity.Api.Services;

public interface IIdentityService
{
    Task<string> GenerateJwtToken(ApplicationUser user);

    Task<IdentityResult> RegisterAsync(RegisterModel model, string? correlationId);

    Task<LoginResponseModel> LoginAsync(LoginModel model);

    RefreshToken GenerateRefreshToken();

    Task<Result> ModerateAsync(ModerateUserModel request, string? correlationId);
}
