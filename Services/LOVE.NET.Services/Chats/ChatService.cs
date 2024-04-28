namespace LOVE.NET.Services.Chats
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Chat;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Chat;

    using Microsoft.EntityFrameworkCore;

    using static LOVE.NET.Common.GlobalConstants;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

    public class ChatService : IChatService
    {
        private readonly IChatRepository chatRepository;
        private readonly IChatroomRepository chatroomRepository;

        // Use Redis Cache for this
        private readonly Dictionary<string, HashSet<UserInRoomModel>> usersInRooms = new();

        public ChatService(
            IChatRepository chatRepository,
            IChatroomRepository chatroomRepository)
        {
            this.chatRepository = chatRepository;
            this.chatroomRepository = chatroomRepository;
        }

        public ChatViewModel GetChat(ChatRequestViewModel request)
        {
            // Add skip and take
            var messagesQuery = this.chatRepository
                .AllAsNoTracking(m => m.RoomId == request.RoomId)
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedOn);

            var messages = messagesQuery
                .Skip((request.Page - 1) * DefaultTake)
                .Take(DefaultTake)
                .To<MessageDto>();

            var count = messagesQuery.Count();

            return new ChatViewModel()
            {
                Messages = messages,
                TotalMessages = count,
            };
        }

        public async Task SaveMessageAsync(MessageDto message)
        {
            var data = AutoMapperConfig.MapperInstance.Map<Message>(message);

            await this.chatRepository.SaveMessageAsync(data);
        }

        public IEnumerable<ChatroomViewModel> GetChatrooms()
        {
            var result = this.chatroomRepository
                .AllAsNoTracking()
                .To<ChatroomViewModel>();
            return result;
        }

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

            this.usersInRooms[connection.RoomId].Add(new UserInRoomModel()
            {
                Id = connection.UserId,
                ProfilePictureUrl = connection.ProfilePictureUrl,
            });
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
