namespace LOVE.NET.Web.ViewModels.Identity
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.Infrastructure.Attributes;
    using Microsoft.AspNetCore.Http;

    using static LOVE.NET.Common.GlobalConstants;

    public class EditUserViewModel : RegisterViewModel, IMapTo<ApplicationUser>
    {
        public string Id { get; set; }
    }
}
