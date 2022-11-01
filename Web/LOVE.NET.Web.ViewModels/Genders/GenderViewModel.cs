namespace LOVE.NET.Web.ViewModels.Genders
{
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class GenderViewModel : IMapFrom<Gender>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
