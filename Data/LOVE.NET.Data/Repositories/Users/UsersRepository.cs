namespace LOVE.NET.Data.Repositories.Users
{
    using LOVE.NET.Data.Models;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public class UsersRepository : EfDeletableEntityRepository<ApplicationUser>, IUsersRepository
    {
        public UsersRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public IQueryable<ApplicationUser> WithAllInformation()
        {
            return this.Context.Users
                .Include(u => u.City)
                .Include(u => u.Country)
                .Include(u => u.LikesReceived)
                .Include(u => u.LikesSent)
                .Include(u => u.Matches);
        }

        public IQueryable<ApplicationUser> WithAllInformation(
            Expression<Func<ApplicationUser, bool>> func)
        {
            return this.Context.Users
                .Include(u => u.City)
                .Include(u => u.Country)
                .Include(u => u.LikesReceived)
                .Include(u => u.LikesSent)
                .Include(u => u.Matches)
                .Where(func);
        }
    }
}
