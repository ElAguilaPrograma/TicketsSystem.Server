
using TicketsSystem.Core.DTOs.UserDTO;

namespace TicketsSystem.Core.Interfaces
{
    public interface ICurrentUserService
    {
        string? GetCurrentUserEmail();
        Guid GetCurrentUserId();
        Task<string> GetCurrentUserName();
        string? GetCurrentUserRole();
    }
}
