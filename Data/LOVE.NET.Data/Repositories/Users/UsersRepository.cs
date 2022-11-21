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
                .Include(u => u.RefreshTokens);
        }

        public IQueryable<ApplicationUser> WithAllInformation(
            Expression<Func<ApplicationUser, bool>> func)
        {
            return this.Context.Users
                .Include(u => u.City)
                .Include(u => u.Country)
                .Include(u => u.Images
                    .Where(i => !i.IsDeleted)
                    .OrderByDescending(i => i.IsProfilePicture))
                .Include(u => u.LikesSent)
                    .ThenInclude(u => u.LikedUser)
                        .ThenInclude(lu => lu.City)
                        .ThenInclude(lu => lu.Country)
                .Include(u => u.LikesSent)
                    .ThenInclude(u => u.LikedUser)
                    .ThenInclude(lu => lu.Gender)
                .Include(u => u.LikesSent)
                    .ThenInclude(u => u.LikedUser)
                    .ThenInclude(lu => lu.Images)
                .Include(u => u.LikesSent)
                    .Where(u => !u.IsDeleted)
                .Include(u => u.RefreshTokens)
                .Include(u => u.LikesReceived)
                .Include(lu => lu.Gender)
                .Include(u => u.Roles)
                .Where(func);
        }
    }
}
