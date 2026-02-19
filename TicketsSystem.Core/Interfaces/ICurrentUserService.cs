
namespace TicketsSystem.Core.Interfaces
{
    public interface ICurrentUserService
    {
        string? GetCurrentUserEmail();
        Guid GetCurrentUserId();
        string? GetCurrentUserRole();
    }
}
