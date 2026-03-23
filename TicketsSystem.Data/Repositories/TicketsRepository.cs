using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data.Repositories;

public class TicketsRepository : GenericRepository<Ticket>, ITicketsRepository
{
    private readonly DbSet<Ticket> _tickets;
    // _context se hereda desde GenericRepository ya que esta como protected
    public TicketsRepository(SystemTicketsContext context) : base(context) 
    {
        _tickets = _dbSet;
    }

    public async Task<(IEnumerable<Ticket> Tickets, int TotalCount)> GetAllTicketsPaginatedWithFilters(
        int page, 
        int pageSize, 
        string? status = null, 
        string? priority = null, 
        string? querySearch = null,
        int? month = null,
        int? year = null,
        Guid? userId = null)
    {
        var query = _tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        if (userId != null)
            query = query.Where(t => t.CreatedByUserId == userId);
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            query = query.Where(t => t.Status.Name == status);
        if (!string.IsNullOrWhiteSpace(priority) && priority != "All")
            query = query.Where(t => t.Priority.Name == priority);
        if (month != null)
            query = query.Where(t => t.CreatedAt.Month == month);
        if (year != null)
            query = query.Where(t => t.CreatedAt.Year == year);
        if (!string.IsNullOrWhiteSpace(querySearch))
        {
            querySearch = querySearch.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(querySearch));
        }

        var totalCount = await query.CountAsync();
        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (tickets, totalCount);
    }

    public async Task<IEnumerable<Ticket>> GetCurrentUserTickets(Guid currentUserId, string userRole)
    {
        var queryable = _tickets.Where(t => t.CreatedByUserId == currentUserId || currentUserId == t.AssignedToUserId);

        if (userRole != "Agent")
            queryable = queryable.Where(t => t.CreatedByUserId == currentUserId);

        return await queryable
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByUserId(Guid userId)
    {
        return await _tickets
            .Where(t => t.CreatedByUserId == userId)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketById(Guid ticketId)
    {
        return await _tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    }

    public Task<bool> TicketExist(Guid ticketId)
        => _tickets.AnyAsync(t  => t.TicketId == ticketId);

    public async Task<Dictionary<int, int>> GetTicketsCountSummary(Guid userId, string userRole)
    {
        var query = _tickets.AsQueryable();

        if (userRole == "Agent")
            query = query.Where(t => t.CreatedByUserId == userId || t.AssignedToUserId == userId);
        else
            query = query.Where(t => t.CreatedByUserId == userId);

        return await query
            .GroupBy(t => t.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StatusId, x => x.Count);
    }
}
