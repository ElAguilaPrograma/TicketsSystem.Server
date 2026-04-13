using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces;

public interface ITicketsRepository : IGenericRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetCurrentUserTickets(Guid currentUserId, string userRole);
    Task<IEnumerable<Ticket>> GetTicketsByUserId(Guid userId);
    Task<Ticket?> GetTicketById(Guid ticketId);
    Task<bool> TicketExist(Guid ticketId);
    Task<(IEnumerable<Ticket> Tickets, int TotalCount)> GetAllTicketsPaginatedWithFilters(int page, int pageSize, string? status = null, string? priority = null, string? querySearch = null, int? month = null, int? year = null, Guid? userId = null, bool? hasAssignment = null, Guid? assignedToUserId = null);
    Task<IEnumerable<Ticket>> ExportTicketsWithFilters(string? status = null, string? priority = null, string? querySearch = null, int? month = null, int? year = null, Guid? userId = null, bool? hasAssignment = null, Guid? assignedToUserId = null);
    Task<Dictionary<int, int>> GetTicketsCountSummary(Guid userId, string userRole);
    Task<int> GetTodaysTicketsCount();
}
