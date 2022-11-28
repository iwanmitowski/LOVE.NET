using System.Threading.Tasks;

using LOVE.NET.Data.Models;

using Microsoft.AspNetCore.SignalR;

namespace LOVE.NET.Services.Chats
{
    public class ChatHub : Hub
    {
        public async Task JoinRoom(UserConnection userConnection)
        {
            await this.Groups
                .AddToGroupAsync(this.Context.ConnectionId, userConnection.RoomId);

            await this.Clients
                .Group(userConnection.RoomId)
                .SendAsync("ReceiveMessage", "userId", "MESSAGEEE");
        }
    }
}
