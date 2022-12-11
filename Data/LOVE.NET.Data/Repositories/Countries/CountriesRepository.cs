namespace LOVE.NET.Data.Repositories.Countries
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using LOVE.NET.Data.Models;

    using Microsoft.EntityFrameworkCore;

    public class CountriesRepository : EfRepository<Country>, ICountriesRepository
    {
        public CountriesRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public IQueryable<Country> WithAllInformation(
            Expression<Func<Country, bool>> func)
        {
            return this.Context.Countries
                .AsNoTracking()
                .Include(c => c.Cities)
                .Where(func);
        }
    }
}
