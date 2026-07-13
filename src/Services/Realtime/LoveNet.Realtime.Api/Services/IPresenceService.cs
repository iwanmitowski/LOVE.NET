using LoveNet.Realtime.Api.Models;

namespace LoveNet.Realtime.Api.Services;

public interface IPresenceService
{
    Task AddUserToRoomAsync(string roomId, UserInRoomModel user);

    Task RemoveUserFromRoomAsync(string roomId, string userId);

    Task<List<UserInRoomModel>> GetUsersInRoomAsync(string roomId);
}
