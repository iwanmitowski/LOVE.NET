using LoveNet.Profile.Api.Models;

namespace LoveNet.Profile.Api.Services;

public interface IProfileService
{
    Task<UserDetailsModel?> GetUserDetailsAsync(string userId);

    Task EditUserAsync(EditUserModel model, string? correlationId);

    Task<AdminUsersResponseModel> GetUsersForAdminAsync(AdminUsersRequestModel request, string loggedUserId);

    Task<bool> CityBelongsToCountryAsync(int cityId, int countryId);
}
