using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> Login(string email);
    Task<bool> EmailExist(string email);
    Task<IEnumerable<User>> SearchUsers(string query);
    Task<IEnumerable<Ticket>> UserManagedTickets(Guid userId);
    Task<(IEnumerable<User> Users, int TotalCount)> GetAllPaginatedWithFilters(int page, int pageSize, string? role = null, bool? isActive = null, string? searchQuery = null);
    Task<IEnumerable<User>> ExportUsersWithFilters(string? role = null, bool? isActive = null);
}
