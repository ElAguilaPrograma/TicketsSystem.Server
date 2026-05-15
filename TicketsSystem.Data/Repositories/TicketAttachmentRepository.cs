using TicketsSystem.Domain.Interfaces;
using TicketsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TicketsSystem.Data.Repositories
{
    public class TicketAttachmentRepository : GenericRepository<TicketAttachment>, ITicketAttachmentRepository
    {
        private readonly DbSet<TicketAttachment> _ticketAttachments;
        public TicketAttachmentRepository(SystemTicketsContext context) : base(context)
        {
            _ticketAttachments = _dbSet;
        }

        public async Task<IEnumerable<TicketAttachment>> GetTicketAttachmentsByTicketId(Guid ticketId)
        {
            var query = _ticketAttachments.Include(ta => ta.Ticket).AsQueryable();
            query = query.Where(ta => ta.TicketId == ticketId);
            var attachments = await query.ToListAsync();
            return attachments;
        }

        public async Task DeleteAttachmentFromTicket(Guid ticketId, Guid attachmentId)
        {
            var attachment = await _ticketAttachments
                .FirstOrDefaultAsync(ta => ta.TicketId == ticketId && ta.TicketAttachmentId == attachmentId);

            if (attachment != null)
            {
                _ticketAttachments.Remove(attachment);
            }
        }
    }
}