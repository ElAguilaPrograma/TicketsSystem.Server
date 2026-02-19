using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly DbSet<User> _users;

    public UserRepository(SystemTicketsContext systemTicketsContext) : base(systemTicketsContext)
    {
        _users = _dbSet;
    }

    public Task<User?> Login(string email)
        => _users.FirstOrDefaultAsync(e => e.Email == email);

    public async Task<IEnumerable<Ticket>> UserManagedTickets(Guid userId)
        => await _context.Tickets.Where(t => t.AssignedToUserId == userId).ToListAsync();

    public Task<bool> EmailExist(string email)
        => _users.AnyAsync(u => u.Email == email);

    public async Task<IEnumerable<User>> SearchUsers(string query)
    {
        return await _users
            .Where(u => u.Email.ToLower().Contains(query) || u.FullName.ToLower().Contains(query))
            .ToListAsync();
    }
}
