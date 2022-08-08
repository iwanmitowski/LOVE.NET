namespace LOVE.NET.Web.Areas.Administration.Controllers
{
    using LOVE.NET.Common;
    using LOVE.NET.Web.Controllers;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
    [Area("Administration")]
    public class AdministrationController : BaseController
    {
    }
}
