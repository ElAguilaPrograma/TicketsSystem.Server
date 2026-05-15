using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces
{
    public interface ITicketAttachmentRepository : IGenericRepository<TicketAttachment>
    {
        Task<IEnumerable<TicketAttachment>> GetTicketAttachmentsByTicketId(Guid ticketId);
        Task DeleteAttachmentFromTicket(Guid ticketId, Guid attachmentId);
    }
}