namespace LOVE.NET.Services.Dating
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LOVE.NET.Data.Repositories.Users;
    using LOVE.NET.Services.Mapping;
    using LOVE.NET.Web.ViewModels.Identity;

    public class DatingService : IDatingService
    {
        private readonly IUsersRepository usersRepository;

        public DatingService(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
        }

        public async Task<IEnumerable<UserMatchViewModel>> GetNotSwipedUsersAsync(string userId)
        {
            var notSwipedUsers = this.usersRepository
                .WithAllInformation(u =>
                    u.Id != userId &&
                    u.Roles.Any() == false && // Excluding admins
                    u.LikesReceived.Any(lr => lr.UserId == userId) == false)
                .To<UserMatchViewModel>();

            return notSwipedUsers;
        }
    }
}
