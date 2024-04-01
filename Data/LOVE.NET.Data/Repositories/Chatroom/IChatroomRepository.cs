namespace LOVE.NET.Data.Repositories.Chat
{
    using System.Linq;

    using LOVE.NET.Data.Models;

    public interface IChatroomRepository
    {
        IQueryable<Chatroom> AllAsNoTracking();
    }
}
