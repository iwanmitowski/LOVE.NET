namespace LoveNet.Realtime.Api.Services;

public interface IRoomRegistry
{
    Task AddPublicRoomAsync(string roomId);

    Task<bool> IsPublicRoomAsync(string roomId);
}
