namespace LOVE.NET.Services.Chats
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Images;
    using LOVE.NET.Web.ViewModels.Chat;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;

    public class ChatHub : Hub
    {
        private readonly IChatService chatService;
        private readonly IImagesService imagesService;

        public ChatHub(
            IChatService chatService,
            IImagesService imagesService)
        {
            this.chatService = chatService;
            this.imagesService = imagesService;
        }

        public async Task JoinRoom(UserConnection userConnection)
        {
            await this.Groups
                .AddToGroupAsync(this.Context.ConnectionId, userConnection.RoomId);
            this.chatService.AddUserToRoom(userConnection);
            await this.Clients
                .Group(userConnection.RoomId)
                .SendAsync("RefreshUsersList", this.chatService.GetUsersInRoom(userConnection.RoomId));
        }

        /// <summary>
        /// Leave room on chat component unmount.
        /// </summary>
        /// <param name="userConnection">User connection input.</param>
        public async Task LeaveRoom(UserConnection userConnection)
        {
            this.chatService.RemoveUserFromRoom(userConnection);
            await this.Clients
                .Group(userConnection.RoomId)
                .SendAsync("RefreshUsersList", this.chatService.GetUsersInRoom(userConnection.RoomId));
        }

        public async Task SendMessage(MessageDto message)
        {
            message.CreatedOn = DateTime.UtcNow;

            if (message.Image != null)
            {
                byte[] bytes = Convert.FromBase64String(message.Image);
                MemoryStream stream = new MemoryStream(bytes);
                IFormFile file = new FormFile(stream, 0, bytes.Length, string.Empty, string.Empty);

                var imageUrl = await this.imagesService.UploadImageAsync(file);
                message.ImageUrl = imageUrl;
            }

            await this.Clients
                .Group(message.RoomId)
                .SendAsync("ReceiveMessage", message);

            await this.chatService.SaveMessageAsync(message);
        }
    }
}
