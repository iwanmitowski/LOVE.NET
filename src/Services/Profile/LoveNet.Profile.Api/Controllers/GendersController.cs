using LoveNet.Profile.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoveNet.Profile.Api.Controllers;

[Route("api/genders")]
[ApiController]
public class GendersController : ControllerBase
{
    private readonly IGendersService gendersService;

    public GendersController(IGendersService gendersService)
    {
        this.gendersService = gendersService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await gendersService.GetAllAsync());
}
