namespace LOVE.NET.Services.Chats
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Web.ViewModels.Chat;

    public interface IChatService
    {
        public ChatViewModel GetChat(ChatRequestViewModel request);

        public Task SaveMessageAsync(MessageDto message);
    }
}
