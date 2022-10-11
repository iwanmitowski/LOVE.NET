namespace LOVE.NET.Web.Controllers
{
    using LOVE.NET.Services.Countries;
    using LOVE.NET.Web.ViewModels.Countries;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;

    [Route(CountriesControllerName)]
    public class CountriesController : ControllerBase
    {
        private readonly ICountriesService countriesService;

        public CountriesController(ICountriesService countriesService)
        {
            this.countriesService = countriesService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CountryViewModel>))]
        public IActionResult GetAll()
        {
            var countryCities = this.countriesService.GetAll();

            return this.Ok(countryCities);
        }

        [HttpGet]
        [Route(ById)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CountryCitiesViewModel))]
        public IActionResult GetAllWithCities(int id)
        {
            var countryCities = this.countriesService.Get(id);

            if (countryCities == null)
            {
                return this.NotFound();
            }

            return this.Ok(countryCities);
        }
    }
}
