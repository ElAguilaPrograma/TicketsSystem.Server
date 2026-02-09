using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SystemTicketsContext _context;

    public UserRepository(SystemTicketsContext systemTicketsContext)
    {
        _context = systemTicketsContext;
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
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

    public Task<User?> Login(string email)
        => _context.Users.FirstOrDefaultAsync(e => e.Email == email);

    public Task<User?> GetUserById(Guid? userId)
        => _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<IEnumerable<Ticket>> UserManagedTickets(Guid userId)
        => await _context.Tickets.Where(t => t.AssignedToUserId == userId).ToListAsync();

    public Task<bool> EmailExist(string email)
        => _context.Users.AnyAsync(u => u.Email == email);

    public async Task<IEnumerable<User>> SearchUsers(string query)
    {
        return await _context.Users
            .Where(u => u.Email.ToLower().Contains(query) || u.FullName.ToLower().Contains(query))
            .ToListAsync();
    }
}
