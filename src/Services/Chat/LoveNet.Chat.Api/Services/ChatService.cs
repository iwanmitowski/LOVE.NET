using LoveNet.Chat.Api.Data;
using LoveNet.Chat.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LoveNet.Chat.Api.Services;

public interface IChatService
{
    Task<ChatResponseModel> GetChatAsync(ChatRequestModel request);

    Task<List<ChatroomModel>> GetChatroomsAsync();

    /// <summary>
    /// A user may read a room when it is a public chatroom, a provisioned
    /// match room they belong to, or (fallback while the match event is still
    /// in flight) a room id containing their own user id.
    /// </summary>
    Task<bool> CanAccessRoomAsync(string roomId, string userId);
}

public class ChatService : IChatService
{
    private readonly ChatDbContext db;
    private readonly IMessageStore messageStore;

    public ChatService(ChatDbContext db, IMessageStore messageStore)
    {
        this.db = db;
        this.messageStore = messageStore;
    }

    public Task<ChatResponseModel> GetChatAsync(ChatRequestModel request) =>
        messageStore.GetPageAsync(request.RoomId, request.Page);

    public Task<List<ChatroomModel>> GetChatroomsAsync() =>
        db.Chatrooms.AsNoTracking()
            .Select(c => new ChatroomModel { Id = c.Id, Title = c.Title, Url = c.Url })
            .ToListAsync();

    public async Task<bool> CanAccessRoomAsync(string roomId, string userId)
    {
        if (roomId.Contains(userId))
        {
            return true;
        }

        if (await db.Chatrooms.AsNoTracking().AnyAsync(c => c.Id == roomId))
        {
            return true;
        }

        return await db.Rooms.AsNoTracking()
            .AnyAsync(r => r.RoomId == roomId && (r.UserAId == userId || r.UserBId == userId));
    }
}
