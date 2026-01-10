using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TicketsSystem.Data.DTOs;

namespace TicketsSystem_Data.Repositories
{
    public interface IUserRepository
    {
        Task CreateNewUser(User newUser);
        Task DeleteUser(User userInfo);
        Task<bool> EmailExist(string email);
        Task<IEnumerable<User>> GetAllUsers();
        Task<User?> GetUserById(Guid userId);
        Task<User?> Login(string email);
        Task UpdateUserInfo(User userInfo);
    }

    public class UserRepository: IUserRepository
    {
        private readonly SystemTicketsContext _context;

        public UserRepository(SystemTicketsContext systemTicketsContext)
        {
            _context = systemTicketsContext;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }

        public Task CreateNewUser(User newUser)
        {
            _context.Users.Add(newUser);
            return _context.SaveChangesAsync();
        }

        public Task UpdateUserInfo(User userInfo)
        {
            _context.Update(userInfo);
            return _context.SaveChangesAsync();
        }

        public Task DeleteUser(User userInfo)
        {
            _context.Users.Remove(userInfo);
            return _context.SaveChangesAsync();
        }

        public Task<User?> Login(string email)
            => _context.Users.FirstOrDefaultAsync(e =>  e.Email == email);

        public Task<User?> GetUserById(Guid userId)
            => _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        public Task<bool> EmailExist(string email)
            => _context.Users.AnyAsync(u => u.Email == email);
    }
}
