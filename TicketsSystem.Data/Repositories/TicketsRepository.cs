using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data.Repositories;

public class TicketsRepository : GenericRepository<Ticket>, ITicketsRepository
{
    private readonly DbSet<Ticket> _tickets;
    private readonly DbSet<TicketComment> _ticketsComment;
    // _context se hereda desde GenericRepository ya que esta como protected
    public TicketsRepository(SystemTicketsContext context) : base(context) 
    {
        _tickets = _dbSet;
        _ticketsComment = _context.Set<TicketComment>();
    }

    public async Task<IEnumerable<Ticket>> GetAllTickets()
    {
        var tickets = await _context.Tickets
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.AssignedToUser)
            .ToListAsync();
        return tickets;
    }

    public async Task<IEnumerable<Ticket>> GetCurrentUserTickets(Guid currentUserId, string userRole)
    {
        var queryable = _context.Tickets.Where(t => t.CreatedByUserId == currentUserId || currentUserId == t.AssignedToUserId);

        if (userRole != "Agent")
            queryable = queryable.Where(t => t.CreatedByUserId == currentUserId);

        return await queryable
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByUserId(Guid userId)
    {
        return await _context.Tickets
            .Where(t => t.CreatedByUserId == userId)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketById(Guid ticketId)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    }

    public async Task<IEnumerable<Ticket?>> SearchTickets(string query, int? statusId, int? priorityId)
    {
        var queryable = _context.Tickets.Where(t => t.Title.ToLower().Contains(query));

        if (statusId.HasValue)
            queryable = queryable.Where(t => t.StatusId == statusId.Value);

        if (priorityId.HasValue)
            queryable = queryable.Where(t => t.PriorityId == priorityId.Value);

        return await queryable
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .ToListAsync();
    }

    public Task<bool> TicketExist(Guid ticketId)
        => _context.Tickets.AnyAsync(t  => t.TicketId == ticketId);

}
