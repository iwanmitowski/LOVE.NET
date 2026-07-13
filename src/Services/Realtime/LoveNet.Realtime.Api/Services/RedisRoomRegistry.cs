using StackExchange.Redis;

namespace LoveNet.Realtime.Api.Services;

/// <summary>Public room ids in a Redis set, populated from RoomProvisioned events.</summary>
public class RedisRoomRegistry : IRoomRegistry
{
    private const string PublicRoomsKey = "rooms:public";

    private readonly IConnectionMultiplexer redis;

    public RedisRoomRegistry(IConnectionMultiplexer redis)
    {
        this.redis = redis;
    }

    public Task AddPublicRoomAsync(string roomId) =>
        redis.GetDatabase().SetAddAsync(PublicRoomsKey, roomId);

    public Task<bool> IsPublicRoomAsync(string roomId) =>
        redis.GetDatabase().SetContainsAsync(PublicRoomsKey, roomId);
}
