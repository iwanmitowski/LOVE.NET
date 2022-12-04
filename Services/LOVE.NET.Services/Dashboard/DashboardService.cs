namespace LOVE.NET.Services.Dashboard
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Common.Repositories;
    using LOVE.NET.Data.Models;
    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Dashboard;
    using LOVE.NET.Web.ViewModels.Identity;

    using Microsoft.EntityFrameworkCore;

    using static LOVE.NET.Common.GlobalConstants;

    public class DashboardService : IDashboardService
    {
        private readonly IUsersRepository usersRepository;
        private readonly IRepository<Image> imagesRepository;
        private readonly IRepository<Message> messagesRepository;

        public DashboardService(
            IUsersRepository usersRepository,
            IRepository<Image> imagesRepository,
            IRepository<Message> messagesRepository)
        {
            this.usersRepository = usersRepository;
            this.imagesRepository = imagesRepository;
            this.messagesRepository = messagesRepository;
        }

        public async Task<StatisticsViewModel> GetStatisticsAsync()
        {
            var usersCount = await this.usersRepository.AllAsNoTracking().CountAsync();
            var bannedUsersCount = await this.usersRepository
                .WithAllInformation(u => u.LockoutEnd != null)
                .CountAsync();
            var matchesCount = await this.usersRepository
                .AllAsNoTracking().CountAsync(user =>
                    user.LikesSent.Any(l =>
                            user.LikesReceived
                                .Select(lr => lr.UserId)
                                .Intersect(
                                    user.LikesSent
                                        .Select(ls => ls.LikedUserId))
                                .Contains(l.LikedUserId)));
            var likedUsersCount = await this.usersRepository.AllAsNoTracking()
                .CountAsync(u => u.LikesReceived.Any());
            var notSwipedUsers = usersCount - likedUsersCount;
            var imagesCount = await this.imagesRepository.AllAsNoTracking().CountAsync();
            var messagesCount = await this.messagesRepository.AllAsNoTracking().CountAsync();

            var result = new StatisticsViewModel()
            {
                UsersCount = usersCount,
                BannedUsersCount = bannedUsersCount,
                MatchesCount = matchesCount,
                LikedUsersCount = likedUsersCount,
                NotSwipedUsersCount = notSwipedUsers,
                ImagesCount = imagesCount,
                MessagesCount = messagesCount,
            };

            return result;
        }

        public async Task<DashboardUserViewModel> GetUsersAsync(
            DashboardUserRequestViewModel request,
            string loggedUserId)
        {
            var users = this.usersRepository.AllAsNoTracking()
                .Where(u =>
                    u.Email != AdministratorEmail &&
                    u.Id != loggedUserId);

            if (request.Search != null)
            {
                var searchTerm = $"%{request.Search.ToLower()}%";

                users = users.Where(u =>
                    EF.Functions.Like(u.Email.ToLower(), searchTerm) ||
                    EF.Functions.Like(u.UserName.ToLower(), searchTerm) ||
                    EF.Functions.Like(u.Bio.ToLower(), searchTerm) ||
                    EF.Functions.Like(u.City.Name.ToLower(), searchTerm) ||
                    EF.Functions.Like(u.Country.Name.ToLower(), searchTerm) ||
                    EF.Functions.Like(u.Gender.Name.ToLower(), searchTerm));
            }

            if (request.ShowBanned)
            {
                users = users.Where(u => u.LockoutEnd != null);
            }

            var totalUsers = users.Count();

            users = users.OrderByDescending(u => u.CreatedOn)
                .Skip((request.Page - 1) * DefaultTake)
                .Take(DefaultTake);

            var mappedUsers = await users.To<UserDetailsViewModel>().ToListAsync();

            var result = new DashboardUserViewModel()
            {
                Users = mappedUsers,
                TotalUsers = totalUsers,
            };

            return result;
        }
    }
}
