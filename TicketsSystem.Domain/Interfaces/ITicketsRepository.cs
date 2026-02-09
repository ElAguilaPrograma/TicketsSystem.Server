using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces;

public interface ITicketsRepository : IGenericRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetAllTickets();
    Task<IEnumerable<Ticket>> GetCurrentUserTickets(Guid currentUserId, string userRole);
    Task<IEnumerable<Ticket>> GetTicketsByUserId(Guid userId);
    Task<Ticket?> GetTicketById(Guid ticketId);
    Task<IEnumerable<Ticket?>> SearchTickets(string query, int? statusId, int? priorityId);
    Task<bool> TicketExist(Guid ticketId);
}
