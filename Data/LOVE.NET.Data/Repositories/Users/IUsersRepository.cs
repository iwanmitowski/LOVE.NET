namespace LOVE.NET.Data.Repositories.Users
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using LOVE.NET.Data.Common.Repositories;

    using LOVE.NET.Data.Models;

    public interface IUsersRepository : IDeletableEntityRepository<ApplicationUser>
    {
        IQueryable<ApplicationUser> WithAllInformation();

        IQueryable<ApplicationUser> WithAllInformation(
            Expression<Func<ApplicationUser, bool>> func);
    }
}
