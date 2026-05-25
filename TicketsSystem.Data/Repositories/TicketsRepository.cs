using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Enums;
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
        Guid? userId = null,
        bool? hasAssignment = null,
        Guid? assignedToUserId = null)
    {
        var query = _tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        if (userId != null)
            query = query.Where(t => t.CreatedByUserId == userId);
        if (assignedToUserId != null)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);
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

        if (hasAssignment.HasValue)
        {
            if (hasAssignment.Value)
                query = query.Where(t => t.AssignedToUserId != null);
            else
                query = query.Where(t => t.AssignedToUserId == null);
        }

        var totalCount = await query.CountAsync();
        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (tickets, totalCount);
    }

    public async Task<IEnumerable<Ticket>> ExportTicketsWithFilters(
        string? status = null, 
        string? priority = null, 
        string? querySearch = null,
        int? month = null,
        int? year = null,
        Guid? userId = null,
        bool? hasAssignment = null,
        Guid? assignedToUserId = null)
    {
        var query = _tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        if (userId != null)
            query = query.Where(t => t.CreatedByUserId == userId);
        if (assignedToUserId != null)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);
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

        if (hasAssignment.HasValue)
        {
            if (hasAssignment.Value)
                query = query.Where(t => t.AssignedToUserId != null);
            else
                query = query.Where(t => t.AssignedToUserId == null);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
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

    public async Task<int> GetTodaysTicketsCount()
    {
        var today = DateTime.UtcNow.Date;
        return await _tickets.CountAsync(t => t.CreatedAt.Date == today);
    }

    public async Task<Dictionary<string, int>> GetTicketsCountByStatus(DateTime? fromDate, DateTime? toDate, Guid? userId, Guid? assignedToUserId)
    {
        var query = _tickets
            .Include(t => t.Status)
            .AsQueryable();

        if (userId != null)
            query = query.Where(t => t.CreatedByUserId == userId);
        if (assignedToUserId != null)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);
        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value);

        return await query
            .GroupBy(t => t.Status.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Name, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetTicketsCountByPriority(DateTime? fromDate, DateTime? toDate, Guid? userId, Guid? assignedToUserId)
    {
        var query = _tickets
            .Include(t => t.Priority)
            .AsQueryable();

        if (userId != null)
            query = query.Where(t => t.CreatedByUserId == userId);
        if (assignedToUserId != null)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);
        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value);

        return await query
            .GroupBy(t => t.Priority.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Name, x => x.Count);
    }

    public async Task<double> GetAverageResolutionHours(DateTime? fromDate, DateTime? toDate, Guid? userId, Guid? assignedToUserId)
    {
        var query = _tickets
            .Where(t => t.ClosedAt != null)
            .AsQueryable();

        if (userId != null)
            query = query.Where(t => t.CreatedByUserId == userId);
        if (assignedToUserId != null)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);
        if (fromDate.HasValue)
            query = query.Where(t => t.ClosedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.ClosedAt <= toDate.Value);

        var durations = await query
            .Select(t => new { t.CreatedAt, t.ClosedAt })
            .ToListAsync();

        if (durations.Count == 0)
            return 0;

        var avgSeconds = durations
            .Where(t => t.ClosedAt.HasValue)
            .Select(t => (t.ClosedAt!.Value - t.CreatedAt).TotalSeconds)
            .DefaultIfEmpty(0)
            .Average();

        return avgSeconds / 3600.0;
    }

    public async Task<IEnumerable<Ticket>> GetRecentTickets(int take, DateTime? fromDate, DateTime? toDate, Guid? userId, Guid? assignedToUserId)
    {
        var query = _tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        if (userId != null)
            query = query.Where(t => t.CreatedByUserId == userId);
        if (assignedToUserId != null)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);
        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetResolvedTodayCount(Guid? userId, Guid? assignedToUserId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var query = _tickets
            .Where(t => t.ClosedAt != null)
            .AsQueryable();

        if (userId != null)
            query = query.Where(t => t.CreatedByUserId == userId);
        if (assignedToUserId != null)
            query = query.Where(t => t.AssignedToUserId == assignedToUserId);

        return await query.CountAsync(t => t.ClosedAt >= today && t.ClosedAt < tomorrow);
    }

    public async Task<IEnumerable<Ticket>> GetSimilarTicketsAsync(Guid excludeTicketId, string[] keywords, int take = 10)
    {
        if (keywords == null || keywords.Length == 0)
            return [];

        var query = _tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Where(t => t.TicketId != excludeTicketId);

        foreach (var keyword in keywords)
        {
            var term = keyword.ToLower();
            query = query.Where(t =>
                EF.Functions.ILike(t.Title, $"%{term}%") ||
                EF.Functions.ILike(t.Description, $"%{term}%"));
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetAgingTicketsAsync(int olderThanDays)
    {
        var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);

        return await _tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Where(t => t.StatusId != (int)TicketsStatusValue.Closed && t.CreatedAt <= cutoff)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }
}
