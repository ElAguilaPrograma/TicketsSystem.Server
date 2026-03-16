using FluentResults;
using TicketsSystem.Core.DTOs.PaginationDTO;
using TicketsSystem.Core.DTOs.UserDTO;

namespace TicketsSystem.Core.Interfaces
{
    public interface IUserService
    {
        Task<Result> CreateNewUserAsync(UserCreateDto userCreateDto);
        Task<Result> DeactivateOrActivateAUserAsync(string userIdStr);
        Task<Result<byte[]>> ExportUsersAsync(FilterUsersDto filterUsersDto);
        Task<Result<PagedResult<UserReadDto>>> GetAllUsersWithFilterAsync(GetAllUsersFilterDto fiilterDto);
        Task<Result<CurrentUserDto>> GetCurrentUser();
        Task<Result<UserCountDto>> GetUsersCount();
        Task<Result<LoginSuccessDto>> LoginAsync(LoginRequest request);
        Task<Result<IEnumerable<UserReadDto>>> SearchUserAsync(string query);
        Task<Result> UpdateUserInformationAsync(UserUpdateDto userUpdateDto, string userIdStr);
    }
}
