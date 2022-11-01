namespace LOVE.NET.Services.Genders
{
    using System.Collections.Generic;

    using LOVE.NET.Web.ViewModels.Genders;

    public interface IGendersService
    {
        public IEnumerable<GenderViewModel> GetAll();
    }
}
