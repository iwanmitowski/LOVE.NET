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

    /// <summary>
    /// Conversation-list data (last message, unread count, peer read state)
    /// for the requested rooms. Rooms the user cannot access are omitted.
    /// </summary>
    Task<List<RoomSummaryModel>> GetSummariesAsync(IReadOnlyList<string> roomIds, string userId);
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

    public async Task<List<RoomSummaryModel>> GetSummariesAsync(IReadOnlyList<string> roomIds, string userId)
    {
        var ids = roomIds.Distinct().ToList();

        // Batched lookups; per-room queries below only run for authorized rooms.
        var publicRoomIds = await db.Chatrooms.AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        var rooms = await db.Rooms.AsNoTracking()
            .Where(r => ids.Contains(r.RoomId))
            .ToDictionaryAsync(r => r.RoomId);

        var myReads = await db.RoomReads.AsNoTracking()
            .Where(r => ids.Contains(r.RoomId) && r.UserId == userId)
            .ToDictionaryAsync(r => r.RoomId, r => r.LastReadAt);

        var summaries = new List<RoomSummaryModel>();

        foreach (var roomId in ids)
        {
            var isPublic = publicRoomIds.Contains(roomId);
            var isMember = isPublic
                || roomId.Contains(userId)
                || (rooms.TryGetValue(roomId, out var membership) &&
                    (membership.UserAId == userId || membership.UserBId == userId));

            if (!isMember)
            {
                continue;
            }

            var lastMessage = await db.Messages.AsNoTracking()
                .Where(m => m.RoomId == roomId)
                .OrderByDescending(m => m.CreatedOn)
                .FirstOrDefaultAsync();

            var lastReadAt = myReads.TryGetValue(roomId, out var read) ? read : DateTime.MinValue;

            var unreadCount = await db.Messages.AsNoTracking()
                .CountAsync(m => m.RoomId == roomId && m.SenderId != userId && m.CreatedOn > lastReadAt);

            DateTime? peerLastReadAt = null;

            var peerId = GetPeerId(roomId, userId, rooms);

            if (!isPublic && peerId is not null)
            {
                var peerRead = await db.RoomReads.AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RoomId == roomId && r.UserId == peerId);
                peerLastReadAt = peerRead?.LastReadAt;
            }

            summaries.Add(new RoomSummaryModel
            {
                RoomId = roomId,
                LastMessageText = lastMessage?.Text,
                LastMessageImageUrl = lastMessage?.ImageUrl,
                LastMessageSenderId = lastMessage?.SenderId,
                LastMessageAt = lastMessage?.CreatedOn,
                UnreadCount = unreadCount,
                PeerLastReadAt = peerLastReadAt,
            });
        }

        return summaries;
    }

    /// <summary>Prefers the provisioned Room row; falls back to the deterministic room-id format.</summary>
    private static string? GetPeerId(string roomId, string userId, IReadOnlyDictionary<string, Room> rooms)
    {
        if (rooms.TryGetValue(roomId, out var room))
        {
            if (room.UserAId == userId)
            {
                return room.UserBId;
            }

            if (room.UserBId == userId)
            {
                return room.UserAId;
            }
        }

        if (roomId.Contains(userId))
        {
            var peerId = roomId.Replace(userId, string.Empty);

            if (Guid.TryParse(peerId, out _))
            {
                return peerId;
            }
        }

        return null;
    }
}
