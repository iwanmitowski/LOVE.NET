using System.Text.Json;
using LoveNet.Realtime.Api.Models;
using StackExchange.Redis;

namespace LoveNet.Realtime.Api.Services;

/// <summary>
/// Redis-backed room presence shared across replicas (replaces the monolith's
/// in-memory singleton). Hash per room keyed by user id; the 24h TTL sweeps
/// rooms whose members never disconnected cleanly.
/// </summary>
public class RedisPresenceService : IPresenceService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer redis;

    public RedisPresenceService(IConnectionMultiplexer redis)
    {
        this.redis = redis;
    }

    public async Task AddUserToRoomAsync(string roomId, UserInRoomModel user)
    {
        var db = redis.GetDatabase();
        var key = RoomKey(roomId);

        await db.HashSetAsync(key, user.Id, JsonSerializer.Serialize(user, JsonOptions));
        await db.KeyExpireAsync(key, TimeSpan.FromHours(24));
    }

    public async Task RemoveUserFromRoomAsync(string roomId, string userId)
    {
        var db = redis.GetDatabase();
        await db.HashDeleteAsync(RoomKey(roomId), userId);
    }

    public async Task<List<UserInRoomModel>> GetUsersInRoomAsync(string roomId)
    {
        var db = redis.GetDatabase();
        var entries = await db.HashGetAllAsync(RoomKey(roomId));

        return entries
            .Select(entry => JsonSerializer.Deserialize<UserInRoomModel>(entry.Value.ToString(), JsonOptions)!)
            .ToList();
    }

    private static string RoomKey(string roomId) => $"presence:room:{roomId}";
}
