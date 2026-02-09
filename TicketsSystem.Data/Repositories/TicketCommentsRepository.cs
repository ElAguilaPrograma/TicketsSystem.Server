using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data.Repositories
{
    public class TicketCommentsRepository : GenericRepository<TicketComment>, ITicketCommentsRepository
    {
        public TicketCommentsRepository(SystemTicketsContext systemTicketsContext) : base(systemTicketsContext) { }

        public async Task<IEnumerable<TicketComment>> GetTicketComments(Guid ticketId)
            => await _dbSet.Where(tc => tc.TicketId == ticketId).ToListAsync();
        public async Task<TicketComment?> GetTicketCommentById(Guid ticketCommentId)
            => await _dbSet.FirstOrDefaultAsync(tc => tc.CommentId == ticketCommentId);
    }
}
