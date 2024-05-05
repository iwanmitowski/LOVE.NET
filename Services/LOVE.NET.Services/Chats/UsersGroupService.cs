namespace LOVE.NET.Services.Chats
{
    using System.Collections.Generic;
    using System.Linq;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Web.ViewModels.Chat;


    public class UsersGroupService : IUsersGroupService
    {
        // Use Redis Cache for this
        private readonly Dictionary<string, HashSet<UserInRoomModel>> usersInRooms =
            new();

        public void AddUserToRoom(MessageDto message)
        {
            if (!this.usersInRooms.ContainsKey(message.RoomId))
            {
                this.usersInRooms[message.RoomId] = new HashSet<UserInRoomModel>();
            }

            this.usersInRooms[message.RoomId].Add(new UserInRoomModel()
            {
                Id = message.UserId,
                ProfilePictureUrl = message.ProfilePicture,
            });
        }

        public void AddUserToRoom(UserConnection connection)
        {
            if (!this.usersInRooms.ContainsKey(connection.RoomId))
            {
                this.usersInRooms[connection.RoomId] = new HashSet<UserInRoomModel>();
            }

            if (this.usersInRooms[connection.RoomId]
                .Any(x => x.Id == connection.UserId) == false)
            {
                this.usersInRooms[connection.RoomId].Add(new UserInRoomModel()
                {
                    Id = connection.UserId,
                    ProfilePictureUrl = connection.ProfilePictureUrl,
                });
            }
        }

        public void RemoveUserFromRoom(UserConnection connection)
        {
            if (this.usersInRooms.ContainsKey(connection.RoomId))
            {
                this.usersInRooms[connection.RoomId].RemoveWhere(x => x.Id == connection.UserId);
                if (this.usersInRooms[connection.RoomId].Any() == false)
                {
                    this.usersInRooms.Remove(connection.RoomId);
                }
            }
        }

        public IEnumerable<UserInRoomModel> GetUsersInRoom(string roomId)
        {
            if (this.usersInRooms.ContainsKey(roomId) == false)
            {
                return Enumerable.Empty<UserInRoomModel>();
            }

            return this.usersInRooms[roomId];
        }
    }
}
