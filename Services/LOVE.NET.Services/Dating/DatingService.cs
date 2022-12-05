﻿namespace LOVE.NET.Services.Dating
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using AutoMapper.Configuration.Conventions;

    using LOVE.NET.Common;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Dating;
    using LOVE.NET.Web.ViewModels.Identity;

    using Microsoft.EntityFrameworkCore;

    using static LOVE.NET.Common.GlobalConstants;
    using static LOVE.NET.Common.GlobalConstants.ControllerResponseMessages;

    public class DatingService : IDatingService
    {
        private readonly IUsersRepository usersRepository;

        public DatingService(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
        }

        public IEnumerable<UserMatchViewModel> GetNotSwipedUsers(string userId)
        {
            var notSwipedUsers = this.usersRepository
                .WithAllInformation(u =>
                    u.Id != userId &&
                    u.Roles.Any() == false && // Excluding admins
                    u.LikesReceived.Any(lr => lr.UserId == userId) == false)
                .To<UserMatchViewModel>();

            return notSwipedUsers;
        }

        public MatchesViewModel GetMatches(MatchesRequestViewModel request)
        {
            var user = this.usersRepository.WithAllInformation(u => u.Id == request.UserId).FirstOrDefault();

            var matches = user.LikesSent
                        .Where(l =>
                            user.LikesReceived
                                .Select(lr => lr.UserId)
                                .Intersect(
                                    user.LikesSent
                                        .Select(ls => ls.LikedUserId))
                                .Contains(l.LikedUserId))
                        .OrderByDescending(ls => ls.CreatedOn)
                        .Select(x => AutoMapperConfig.MapperInstance.Map<UserMatchViewModel>(x.LikedUser));

            var totalMatches = matches.Count();

            matches = matches
                .Skip((request.Page - 1) * DefaultTake)
                .Take(DefaultTake)
                .ToList();

            foreach (var match in matches)
            {
                var roomId = string.Join(string.Empty, new[] { match.Id, request.UserId }.OrderBy(id => id));
                match.RoomId = roomId;
            }

            var result = new MatchesViewModel()
            {
                Matches = matches,
                TotalMatches = totalMatches,
            };

            return result;
        }

        public UserMatchViewModel GetCurrentMatch(string userId)
        {
            var match = this.usersRepository
                .WithAllInformation(u =>
                    u.Id == userId)
                .FirstOrDefault();

            var result = AutoMapperConfig.MapperInstance.Map<UserMatchViewModel>(match);
            result.Images = result.Images.OrderByDescending(i => i.IsProfilePicture).ToList();

            return result;
        }

        public async Task<Result> LikeAsync(string userId, string likedUserId)
        {
            var users = this.usersRepository.WithAllInformation(
                u => new[] { userId, likedUserId }.Any(id => id == u.Id));

            var likedUser = await users.FirstOrDefaultAsync(u => u.Id == likedUserId);
            if (likedUser == null)
            {
                return UserNotFound;
            }

            if (users.Count() == 1)
            {
                return YouCantLikeYourself;
            }

            var user = await users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user.LikesSent.Any(ls => ls.LikedUserId == likedUserId))
            {
                return UserAlreadyLiked;
            }

            user.LikesSent.Add(new Like()
            {
                LikedUserId = likedUserId,
                CreatedOn = DateTime.UtcNow,
            });

            await this.usersRepository.SaveChangesAsync();

            var isMatch = likedUser.LikesSent.Any(ls => ls.LikedUserId == userId);

            return isMatch;
        }
    }
}
