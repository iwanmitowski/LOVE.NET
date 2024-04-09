namespace LOVE.NET.Web.ViewModels.Chat
{
    using System;
    using System.Linq;

    using AutoMapper;

    using LOVE.NET.Data.Models;
    using LOVE.NET.Services.Mapping;

    public class MessageDto : IMapFrom<Message>, IMapTo<Message>, IHaveCustomMappings
    {
        public string RoomId { get; set; }

        public string UserId { get; set; }

        public string Text { get; set; }

        /// <summary>
        /// Profile picture of the user who sent the message.
        /// </summary>
        public string ProfilePicture { get; set; }

        /// <summary>
        /// Image as chat message.
        /// </summary>
        public string? ImageUrl { get; set; }

        public string Image { get; set; }

        public DateTime CreatedOn { get; set; }

        public void CreateMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Message, MessageDto>()
                .ForMember(m => m.ProfilePicture, opt => opt
                    .MapFrom(x => x.User.Images
                        .OrderByDescending(i => i.IsProfilePicture)
                        .FirstOrDefault()
                        .Url));
        }
    }
}
