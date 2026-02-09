using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces;

public interface ITicketsRepository : IGenericRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetAllTickets();
    Task<IEnumerable<Ticket>> GetCurrentUserTickets(Guid currentUserId, string userRole);
    Task<IEnumerable<Ticket>> GetTicketsByUserId(Guid userId);
    Task<Ticket?> GetTicketById(Guid ticketId);
    Task<IEnumerable<Ticket?>> SearchTickets(string query, int? statusId, int? priorityId);
    Task CreateTicketComment(TicketComment ticketComment);
    Task<bool> TicketExist(Guid ticketId);
    Task<IEnumerable<TicketComment>> GetTicketComments(Guid ticketId);
    Task UpdateTicketComment(TicketComment ticketComment);
    Task<TicketComment?> GetTicketCommentById(Guid ticketCommentId);
}
