namespace LOVE.NET.Data.Repositories.Chat
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Models;

    public interface IChatRepository
    {
        IQueryable<Message> AllAsNoTracking(Expression<Func<Message, bool>> func);

        Task SaveMessageAsync(Message message);
    }
}
