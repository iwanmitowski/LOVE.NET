namespace LOVE.NET.Services.Chats
{
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
    }
}
