using FluentResults;
using TicketsSystem.Core.DTOs.UserDTO;

namespace TicketsSystem.Core.Interfaces
{
    public interface IUserService
    {
        Task<Result> CreateNewUserAsync(UserCreateDto userCreateDto);
        Task<Result> DeactivateOrActivateAUserAsync(string userIdStr);
        Task<Result<IEnumerable<UserReadDto>>> GetAllUsersAsync();
        Task<Result<LoginSuccessDto>> LoginAsync(LoginRequest request);
        Task<Result<IEnumerable<UserReadDto>>> SearchUserAsync(string query);
        Task<Result> UpdateUserInformationAsync(UserUpdateDto userUpdateDto, string userIdStr);
    }
}
