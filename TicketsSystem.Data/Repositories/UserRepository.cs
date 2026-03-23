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

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetAllPaginatedWithFilters(int page, int pageSize, string? role = null, bool? isActive = null, string? querySearch = null)
    {
        var query = _users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(role) && role != "All Roles")
            query = query.Where(u => u.Role == role);
        if (isActive.HasValue)
            query = query.Where(u =>u.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(querySearch))
        {
            querySearch = querySearch.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(querySearch) || u.Email.ToLower().Contains(querySearch));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<IEnumerable<User>> ExportUsersWithFilters(string? role = null, bool? isActive = null)
    {
        var query = _users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(role) && role != "All Roles")
            query = query.Where(u => u.Role == role);
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var users = await query
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return users;
    }

}
