namespace LOVE.NET.Services.Chats
{
    using System.Collections.Generic;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Web.ViewModels.Chat;

    public interface IUsersGroupService
    {
        void AddUserToRoom(UserConnection connection);

        void RemoveUserFromRoom(UserConnection connection);

        IEnumerable<UserInRoomModel> GetUsersInRoom(string roomId);
    }
}
