namespace LOVE.NET.Data.Repositories.Chat
{
    using System.Linq;

    using LOVE.NET.Data.Models;

    using Microsoft.EntityFrameworkCore;

    public class ChatroomRepository : EfRepository<Chatroom>, IChatroomRepository
    {
        public ChatroomRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}
