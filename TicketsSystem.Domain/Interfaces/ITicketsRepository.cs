using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces;

public interface ITicketsRepository
{
    Task<IEnumerable<Ticket>> GetAllTickets();
    Task<IEnumerable<Ticket>> GetCurrentUserTickets(Guid currentUserId, string userRole);
    Task<IEnumerable<Ticket>> GetTicketsByUserId(Guid userId);
    Task<Ticket?> GetTicketById(Guid ticketId);
    Task CreateTicket(Ticket newTicket);
    Task UpdateTicketInfo(Ticket ticket);
    Task AssingTicket(Ticket ticketWithAssingUserId);
    Task AcceptTickets(Ticket ticketAccepted);
    Task<IEnumerable<Ticket?>> SearchTickets(string query, int? statusId, int? priorityId);
    Task CreateTicketComment(TicketComment ticketComment);
    Task<bool> TicketExist(Guid ticketId);
    Task<IEnumerable<TicketComment>> GetTicketComments(Guid ticketId);
}
