namespace LOVE.NET.Services.Chats
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Chat;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Chat;

    using static LOVE.NET.Common.GlobalConstants;

    public class ChatService : IChatService
    {
        private readonly IChatRepository chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        public ChatViewModel GetChat(ChatRequestViewModel request)
        {
            // Add skip and take
            var messagesQuery = this.chatRepository
                .AllAsNoTracking(m => m.RoomId == request.RoomId)
                .OrderByDescending(x => x.CreatedOn);

            var messages = messagesQuery
                .Skip((request.Page - 1) * DefaultTake)
                .Take(DefaultTake)
                .Select(m => new MessageDto()
                {
                    RoomId = m.RoomId,
                    UserId = m.UserId,
                    CreatedOn = m.CreatedOn,
                    Text = m.Text,
                });

            var count = messagesQuery.Count();

            return new ChatViewModel()
            {
                Messages = messages,
                TotalMessages = count,
            };
        }

        public async Task SaveMessageAsync(MessageDto message)
        {
            var data = new Message()
            {
                RoomId = message.RoomId,
                UserId = message.UserId,
                Text = message.Text,
                CreatedOn = message.CreatedOn,
            };

            await this.chatRepository.SaveMessageAsync(data);
        }
    }
}
