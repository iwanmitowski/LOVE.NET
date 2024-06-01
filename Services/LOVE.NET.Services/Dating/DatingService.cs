namespace LOVE.NET.Services.Dating
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
                    u.LikesReceived.Any(lr => lr.UserId == userId && lr.IsDeleted == false) == false)
                .To<UserMatchViewModel>();

            foreach (var user in notSwipedUsers)
            {
                user.Images = user.Images.OrderByDescending(i => i.IsProfilePicture).ToList();
            }

            return notSwipedUsers;
        }

        public MatchesViewModel GetMatches(MatchesRequestViewModel request)
        {
            var user = this.usersRepository.WithAllInformation(u => u.Id == request.UserId).FirstOrDefault();

            var matchesIds = user.LikesSent
                        .Where(l =>
                            l.IsDeleted == false &&
                            user.LikesReceived
                                .Select(lr => lr.UserId)
                                .Intersect(
                                    user.LikesSent
                                        .Select(ls => ls.LikedUserId))
                                .Contains(l.LikedUserId))
                        .OrderByDescending(ls => ls.CreatedOn)
                        .Select(x => x.LikedUserId)
                        .ToList();

            var matches = this.usersRepository
                .WithAllInformation()
                .To<UserMatchViewModel>();

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = $"%{request.Search.Trim().ToLower()}%";

                matches = matches.Where(u =>
                    EF.Functions.Like(u.UserName.ToLower(), searchTerm) ||
                    EF.Functions.Like(u.Bio.ToLower(), searchTerm) ||
                    EF.Functions.Like(u.CityName.ToLower(), searchTerm) ||
                    EF.Functions.Like(u.CountryName.ToLower(), searchTerm) ||
                    EF.Functions.Like(u.GenderName.ToLower(), searchTerm));
            }

            var matchesResult = matches
               .Where(m => matchesIds.Contains(m.Id))
               .Skip((request.Page - 1) * DefaultTake)
               .Take(DefaultTake)
               .ToList();

            var totalMatches = matchesResult.Count();

            foreach (var match in matchesResult)
            {
                var roomId = string.Join(string.Empty, new[] { match.Id, request.UserId }.OrderBy(id => id));
                match.RoomId = roomId;
                match.Images = match.Images.OrderByDescending(i => i.IsProfilePicture).ToList();
            }

            var result = new MatchesViewModel()
            {
                Matches = matchesResult,
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

            if (user.LikesSent.Any(ls => !ls.IsDeleted && ls.LikedUserId == likedUserId))
            {
                return UserAlreadyLiked;
            }

            user.LikesSent.Add(new Like()
            {
                LikedUserId = likedUserId,
                CreatedOn = DateTime.UtcNow,
            });

            await this.usersRepository.SaveChangesAsync();

            var isMatch = likedUser.LikesSent.Any(ls => !ls.IsDeleted && ls.LikedUserId == userId);

            return isMatch;
        }

        // Refactor this with Like repository
        public async Task<Result> UnlikeAsync(string userId, string unlikedUserId)
        {
            var users = this.usersRepository.WithAllInformation(
                u => new[] { userId, unlikedUserId }.Any(id => id == u.Id));

            var unlikedUserFound = await users.AnyAsync(u => u.Id == unlikedUserId);

            if (!unlikedUserFound)
            {
                return UserNotFound;
            }

            if (users.Count() == 1)
            {
                return YouCantLikeYourself;
            }

            var user = await users.FirstOrDefaultAsync(u => u.Id == userId);

            var previouslyUnliked = user.LikesSent.Any(ls => ls.LikedUserId == unlikedUserId && ls.IsDeleted);
            var noLikesAfterUnlike = user.LikesSent.All(ls => ls.LikedUserId != unlikedUserId || ls.CreatedOn <= ls.DeletedOn);

            if (previouslyUnliked && noLikesAfterUnlike)
            {
                return UserAlreadyUnliked;
            }

            var date = DateTime.UtcNow;
            var activeLike = user.LikesSent
                .OrderByDescending(ls => ls.CreatedOn)
                .FirstOrDefault(ls => ls.LikedUserId == unlikedUserId && !ls.IsDeleted);
            
            if (activeLike == null)
            {
                return UserAlreadyUnliked;
            }

            activeLike.IsDeleted = true;
            activeLike.DeletedOn = date;
            activeLike.ModifiedOn = date;

            await this.usersRepository.SaveChangesAsync();

            return true;
        }
    }
}
