namespace LOVE.NET.Services.Chats
{
    using System;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using Microsoft.AspNetCore.SignalR;

    public class ChatHub : Hub
    {
        private readonly IChatService chatService;

        public ChatHub(IChatService chatService)
        {
            this.chatService = chatService;
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

            await this.chatService.SaveMessageAsync(message);
        }
    }
}
