using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data.Repositories;

public class TicketsRepository : ITicketsRepository
{
    private readonly SystemTicketsContext _context;
    public TicketsRepository(SystemTicketsContext context)
    {
        _context = context;
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

    public Task CreateTicket(Ticket newTicket)
    {
        _context.Tickets.Add(newTicket);
        return _context.SaveChangesAsync();
    }

    public Task UpdateTicketInfo(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        return _context.SaveChangesAsync();
    }

    public Task AssingTicket(Ticket ticketWithAssingUserId)
    {
        _context.Tickets.Update(ticketWithAssingUserId);
        return _context.SaveChangesAsync();
    }

    public Task AcceptTickets(Ticket ticketAccepted)
    {
        _context.Tickets.Update(ticketAccepted);
        return _context.SaveChangesAsync();
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

    public Task CreateTicketComment(TicketComment ticketComment)
    {
        _context.TicketComments.Add(ticketComment);
        return _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TicketComment>> GetTicketComments(Guid ticketId)
        => await _context.TicketComments.Where(tc => tc.TicketId == ticketId).ToListAsync();
}
