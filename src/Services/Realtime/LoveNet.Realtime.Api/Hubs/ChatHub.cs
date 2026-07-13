using LoveNet.BuildingBlocks.Contracts;
using LoveNet.BuildingBlocks.Contracts.Events;
using LoveNet.BuildingBlocks.EventBus;
using LoveNet.BuildingBlocks.Web.Auth;
using LoveNet.Realtime.Api.Models;
using LoveNet.Realtime.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LoveNet.Realtime.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private const string RoomIdKey = "roomId";
    private const string UserIdKey = "userId";

    private readonly IPresenceService presence;
    private readonly IRoomRegistry roomRegistry;
    private readonly IEventPublisher eventPublisher;

    public ChatHub(
        IPresenceService presence,
        IRoomRegistry roomRegistry,
        IEventPublisher eventPublisher)
    {
        this.presence = presence;
        this.roomRegistry = roomRegistry;
        this.eventPublisher = eventPublisher;
    }

    public async Task JoinRoom(UserConnectionModel connection)
    {
        var userId = Context.User!.GetUserId();
        var userName = Context.User!.GetUserName();
        var roomId = connection.RoomId;

        await EnsureRoomMemberAsync(roomId, userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await presence.AddUserToRoomAsync(roomId, new UserInRoomModel
        {
            Id = userId,
            UserName = userName,
            ProfilePictureUrl = connection.ProfilePictureUrl,
        });

        Context.Items[RoomIdKey] = roomId;
        Context.Items[UserIdKey] = userId;

        await BroadcastUsersAsync(roomId, hasLeft: false);
    }

    public async Task LeaveRoom(UserConnectionModel connection)
    {
        var userId = Context.User!.GetUserId();
        await RemoveFromRoomAsync(connection.RoomId, userId);
    }

    /// <summary>
    /// Cleans up presence when the client never invoked LeaveRoom (tab close,
    /// network drop) — the monolith leaked ghost users here.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(RoomIdKey, out var roomId) && roomId is string room &&
            Context.Items.TryGetValue(UserIdKey, out var userId) && userId is string user)
        {
            await RemoveFromRoomAsync(room, user);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(SendMessageModel message)
    {
        var userId = Context.User!.GetUserId();
        var userName = Context.User!.GetUserName();

        await EnsureRoomMemberAsync(message.RoomId, userId);

        var evt = new MessageSentEvent(
            Guid.NewGuid().ToString(),
            message.RoomId,
            userId,
            userName,
            message.ProfilePicture,
            message.Text,
            message.ImageUrl,
            DateTime.UtcNow);

        // Durable-before-display: broker ack first, so a message a client saw
        // can never be lost by a crash between broadcast and publish.
        await eventPublisher.PublishAsync(KafkaTopics.MessageSent, message.RoomId, evt);

        var payload = new
        {
            Id = evt.Id,
            RoomId = message.RoomId,
            UserId = userId,
            Text = message.Text,
            ProfilePicture = message.ProfilePicture,
            ImageUrl = message.ImageUrl,
            CreatedOn = evt.CreatedOn,
        };

        await Clients.Group(message.RoomId).SendAsync("ReceiveMessage", payload);

        // In a 1:1 room the peer may not be joined to the group (thread not
        // open); a direct user push keeps their unread badge/toast live.
        var peerId = GetPeerIdFromRoomId(message.RoomId, userId);

        if (peerId is not null && !await roomRegistry.IsPublicRoomAsync(message.RoomId))
        {
            await Clients.User(peerId).SendAsync("MessageNotification", new
            {
                payload.Id,
                payload.RoomId,
                payload.UserId,
                SenderUserName = userName,
                payload.Text,
                payload.ImageUrl,
                payload.CreatedOn,
            });
        }
    }

    /// <summary>Marks the room read for the caller: durable via Kafka, live via a group broadcast.</summary>
    public async Task MarkRead(string roomId)
    {
        var userId = Context.User!.GetUserId();

        await EnsureRoomMemberAsync(roomId, userId);

        var evt = new RoomReadEvent(roomId, userId, DateTime.UtcNow);

        await eventPublisher.PublishAsync(KafkaTopics.RoomRead, roomId, evt);

        await Clients.OthersInGroup(roomId).SendAsync("ReadReceipt", new
        {
            RoomId = roomId,
            UserId = userId,
            ReadAt = evt.ReadAt,
        });
    }

    /// <summary>Transient typing signal — no persistence, consumers debounce client-side.</summary>
    public async Task Typing(string roomId)
    {
        var userId = Context.User!.GetUserId();

        await EnsureRoomMemberAsync(roomId, userId);

        await Clients.OthersInGroup(roomId).SendAsync("PeerTyping", new
        {
            RoomId = roomId,
            UserId = userId,
            UserName = Context.User!.GetUserName(),
        });
    }

    /// <summary>
    /// The 1:1 room id is both user GUIDs sorted and concatenated, so the peer
    /// id is the room id minus the caller's own id. Public room ids don't
    /// contain the caller's id, yielding null.
    /// </summary>
    private static string? GetPeerIdFromRoomId(string roomId, string userId)
    {
        if (!roomId.Contains(userId))
        {
            return null;
        }

        var peerId = roomId.Replace(userId, string.Empty);

        return Guid.TryParse(peerId, out _) ? peerId : null;
    }

    /// <summary>
    /// A room is joinable when it is a provisioned public room or the caller's
    /// id appears in the deterministic 1:1 room id (two sorted user GUIDs
    /// concatenated).
    /// </summary>
    private async Task EnsureRoomMemberAsync(string roomId, string userId)
    {
        if (await roomRegistry.IsPublicRoomAsync(roomId) || roomId.Contains(userId))
        {
            return;
        }

        throw new HubException("Not a member of this room");
    }

    private async Task RemoveFromRoomAsync(string roomId, string userId)
    {
        await presence.RemoveUserFromRoomAsync(roomId, userId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        Context.Items.Remove(RoomIdKey);
        Context.Items.Remove(UserIdKey);

        await BroadcastUsersAsync(roomId, hasLeft: true);
    }

    private async Task BroadcastUsersAsync(string roomId, bool hasLeft)
    {
        var users = await presence.GetUsersInRoomAsync(roomId);
        await Clients.Group(roomId).SendAsync("RefreshUsersList", new
        {
            Users = users,
            HasLeft = hasLeft,
        });
    }
}
