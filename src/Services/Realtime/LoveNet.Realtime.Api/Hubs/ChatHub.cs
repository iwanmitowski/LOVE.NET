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

        await Clients.Group(message.RoomId).SendAsync("ReceiveMessage", new
        {
            RoomId = message.RoomId,
            UserId = userId,
            Text = message.Text,
            ProfilePicture = message.ProfilePicture,
            ImageUrl = message.ImageUrl,
            CreatedOn = evt.CreatedOn,
        });
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
