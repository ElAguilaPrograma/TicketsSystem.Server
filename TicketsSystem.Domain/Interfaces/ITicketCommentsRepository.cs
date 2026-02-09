using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces
{
    public interface ITicketCommentsRepository : IGenericRepository<TicketComment>
    {
        Task<TicketComment?> GetTicketCommentById(Guid ticketCommentId);
        Task<IEnumerable<TicketComment>> GetTicketComments(Guid ticketId);
    }
}
