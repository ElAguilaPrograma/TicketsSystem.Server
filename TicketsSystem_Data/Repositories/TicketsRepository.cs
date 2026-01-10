using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem_Data;

namespace TicketsSystem.Data.Repositories
{
    public interface ITicketsRepository
    {
        Task AssingTicket(Ticket ticketWithAssingUserId);
        Task CreateTicket(Ticket newTicket);
        Task<IEnumerable<Ticket>> GetAllTickets();
        Task<IEnumerable<Ticket>> GetCurrentUserTickets(Guid currentUserId);
        Task<Ticket?> GetTicketById(Guid ticketId);
        Task<IEnumerable<Ticket>> GetTicketsByUserId(Guid userId);
    }

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
                .ToListAsync();
            return tickets;
        }

        public async Task<IEnumerable<Ticket>> GetCurrentUserTickets(Guid currentUserId)
        {
            return await _context.Tickets
                .Where(t => t.CreatedByUserId == currentUserId)
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
                .FirstOrDefaultAsync(t =>  t.TicketId == ticketId);
        }

        public Task CreateTicket(Ticket newTicket)
        {
            _context.Tickets.Add(newTicket);
            return _context.SaveChangesAsync();
        }

        public Task AssingTicket(Ticket ticketWithAssingUserId)
        {
            _context.Tickets.Update(ticketWithAssingUserId);
            return _context.SaveChangesAsync();
        }
    }
}
