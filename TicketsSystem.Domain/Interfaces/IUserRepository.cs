using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsers();
    Task<User?> GetUserById(Guid userId);
    Task<User?> Login(string email);
    Task<bool> EmailExist(string email);
    Task CreateNewUser(User newUser);
    Task UpdateUserInfo(User userInfo);
    Task<IEnumerable<User>> SearchUsers(string query);
    Task<IEnumerable<Ticket>> UserManagedTickets(Guid userId);
}
