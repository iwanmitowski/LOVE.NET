﻿namespace LOVE.NET.Web.Controllers
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Dating;
    using LOVE.NET.Web.ViewModels.Dating;
    using LOVE.NET.Web.ViewModels.Identity;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    using static LOVE.NET.Common.GlobalConstants.ControllerRoutesConstants;

    [Route(DatingControllerName)]
    [ApiController]
    public class DatingController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IDatingService datingService;

        public DatingController(
            UserManager<ApplicationUser> userManager,
            IDatingService datingService)
        {
            this.userManager = userManager;
            this.datingService = datingService;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserMatchViewModel[]))]
        public async Task<IActionResult> GetNotSwipedUsersAsync()
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = await this.userManager.FindByIdAsync(loggedUserId);

            if (user == null)
            {
                return this.Unauthorized();
            }

            var notSwipedUsers = this.datingService.GetNotSwipedUsers(loggedUserId);

            return this.Ok(notSwipedUsers);
        }

        [HttpPost]
        [Authorize]
        [Route(MatchesRoute)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatchesViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetMatches([FromBody] MatchesRequestViewModel request)
        {
            if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out _))
            {
                return this.BadRequest();
            }

            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (loggedUserId != request.UserId)
            {
                return this.Forbid();
            }

            var result = this.datingService.GetMatches(request);

            return this.Ok(result);
        }

        [HttpPost]
        [Authorize]
        [Route(LikeRoute)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MatchViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(Result))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LikeAsync(string id)
        {
            var loggedUserId = this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var serviceResult = await this.datingService.LikeAsync(loggedUserId, id);

            if (serviceResult.Errors != null)
            {
                return this.BadRequest((Result)string.Join('\n', serviceResult.Errors));
            }

            var result = new MatchViewModel()
            {
                IsMatch = serviceResult.Succeeded,
            };

            if (result.IsMatch)
            {
                var match = this.datingService.GetCurrentMatch(id);
                result.User = match;
            }

            return this.Ok(result);
        }
    }
}
