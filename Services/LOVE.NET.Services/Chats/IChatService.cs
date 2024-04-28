namespace LOVE.NET.Services.Chats
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Web.ViewModels.Chat;

    public interface IChatService
    {
        ChatViewModel GetChat(ChatRequestViewModel request);

        Task SaveMessageAsync(MessageDto message);

        IEnumerable<ChatroomViewModel> GetChatrooms();

        void AddUserToRoom(UserConnection connection);

        void RemoveUserFromRoom(UserConnection connection);

        IEnumerable<UserInRoomModel> GetUsersInRoom(string roomId);
    }
}
