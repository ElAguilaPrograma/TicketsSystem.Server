using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{
    public interface IGetUserRole
    {
        Task<bool> UserIsAdmin(Guid userId);
        Task<bool> UserIsAgent(Guid userId);
        Task<bool> UserIsUser(Guid userId);
    }
    public class GetUserRoleService : IGetUserRole
    {
        private readonly IUserRepository _userRepository;
        public GetUserRoleService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> UserIsAdmin(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null) return false;

            if (user.Role == "Admin") return true;

            return false;
        }

        public async Task<bool> UserIsAgent(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null) return false;

            if (user.Role == "Agent") return true;

            return false;
        }

        public async Task<bool> UserIsUser(Guid userId)
        {
            var user = await _userRepository.GetUserById(userId);
            if (user == null) return false;

            if (user.Role == "User") return true;

            return false;
        }
    }
}
