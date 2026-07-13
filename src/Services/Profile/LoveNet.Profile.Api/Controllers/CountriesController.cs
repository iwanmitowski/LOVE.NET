using LoveNet.Profile.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoveNet.Profile.Api.Controllers;

[Route("api/countries")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly ICountriesService countriesService;

    public CountriesController(ICountriesService countriesService)
    {
        this.countriesService = countriesService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await countriesService.GetAllAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int id)
    {
        var country = await countriesService.GetAsync(id);

        return country is null ? NotFound() : Ok(country);
    }
}
