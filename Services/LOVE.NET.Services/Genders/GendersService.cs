namespace LOVE.NET.Services.Genders
{
    using System.Collections.Generic;

    using LOVE.NET.Data.Common.Repositories;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Genders;

    public class GendersService : IGendersService
    {
        private readonly IRepository<Gender> gendersRepository;

        public GendersService(IRepository<Gender> gendersRepository)
        {
            this.gendersRepository = gendersRepository;
        }

        public IEnumerable<GenderViewModel> GetAll()
        {
            var genders = this.gendersRepository.AllAsNoTracking();

            var result = AutoMapperConfig.MapperInstance.Map<IEnumerable<GenderViewModel>>(genders);

            return result;
        }
    }
}
