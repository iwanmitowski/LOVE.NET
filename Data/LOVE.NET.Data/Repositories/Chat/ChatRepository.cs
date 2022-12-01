namespace LOVE.NET.Data.Repositories.Chat
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    using Microsoft.EntityFrameworkCore;

    public class ChatRepository : EfRepository<Message>, IChatRepository
    {
        public ChatRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public IQueryable<Message> AllAsNoTracking(Expression<Func<Message, bool>> func)
        {
            return this.Context.Messages
               .AsNoTracking()
               .Include(c => c.User)
               .Where(func);
        }

        public async Task SaveMessageAsync(Message message)
        {
            await this.AddAsync(message);
            await this.SaveChangesAsync();
        }
    }
}
