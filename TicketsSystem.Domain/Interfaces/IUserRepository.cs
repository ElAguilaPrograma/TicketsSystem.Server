using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> Login(string email);
    Task<bool> EmailExist(string email);
    Task<IEnumerable<User>> SearchUsers(string query);
    Task<IEnumerable<Ticket>> UserManagedTickets(Guid userId);
}
