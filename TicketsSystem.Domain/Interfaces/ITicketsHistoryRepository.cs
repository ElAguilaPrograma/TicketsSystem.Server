using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces
{
    public interface ITicketsHistoryRepository : IGenericRepository<TicketHistory>
    {
        Task<IEnumerable<TicketHistory>> GetTicketHistories(Guid ticketId);
        Task TrackChanges(Ticket ticket, Guid userId);
    }
}
