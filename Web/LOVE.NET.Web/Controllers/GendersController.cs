namespace LOVE.NET.Web.Controllers
{
    using System.Collections.Generic;

    using LOVE.NET.Services.Genders;
    using LOVE.NET.Web.ViewModels.Genders;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;

    [Route(GendersControllerName)]
    [ApiController]
    public class GendersController : ControllerBase
    {
        private readonly IGendersService gendersService;

        public GendersController(IGendersService gendersService)
        {
            this.gendersService = gendersService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GendersViewModel>))]
        public IActionResult GetAll()
        {
            var genders = this.gendersService.GetAll();

            return this.Ok(genders);
        }
    }
}
