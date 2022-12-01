namespace LOVE.NET.Services.Chats
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Chat;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Chat;
    using LOVE.NET.Web.ViewModels.Identity;

    public class ChatService : IChatService
    {
        private readonly IChatRepository chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            this.chatRepository = chatRepository;
        }

        public IEnumerable<MessageViewModel> GetChat(string roomId)
        {
            // Add skip and take
            var messages = this.chatRepository
                .AllAsNoTracking(m => m.RoomId == roomId)
                .OrderByDescending(x => x.CreatedOn)
                .To<MessageViewModel>();

            return messages;
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
