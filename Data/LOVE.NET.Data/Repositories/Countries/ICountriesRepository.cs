namespace LOVE.NET.Data.Repositories.Countries
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using LOVE.NET.Data.Models;

    public interface ICountriesRepository
    {
        IQueryable<Country> WithAllInformation(Expression<Func<Country, bool>> func);

        IQueryable<Country> AllAsNoTracking();
    }
}
