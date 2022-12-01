namespace LOVE.NET.Services.Chats
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Web.ViewModels.Chat;

    public interface IChatService
    {
        public IEnumerable<MessageViewModel> GetChat(string userId);

        public Task SaveMessageAsync(MessageDto message);
    }
}
