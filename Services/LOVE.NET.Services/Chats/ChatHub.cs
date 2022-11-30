using System;
using System.Threading.Tasks;

using LOVE.NET.Data.Models;
using LOVE.NET.Services.Identity;

using Microsoft.AspNetCore.SignalR;

namespace LOVE.NET.Services.Chats
{
    public class ChatHub : Hub
    {
        private readonly IIdentityService identityService;

        public ChatHub(IIdentityService identityService)
        {
            this.identityService = identityService;
        }

        public async Task JoinRoom(UserConnection userConnection)
        {
            await this.Groups
                .AddToGroupAsync(this.Context.ConnectionId, userConnection.RoomId);
        }

        public async Task SendMessage(MessageDto message)
        {
            message.CreatedOn = DateTime.UtcNow;
            await this.Clients
                .Group(message.RoomId)
                .SendAsync("ReceiveMessage", message);
        }
    }
}
