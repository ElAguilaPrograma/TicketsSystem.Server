using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;


namespace TicketsSystem.Data.Repositories
{
    public class TicketsHistoryRepository : GenericRepository<TicketHistory>, ITicketsHistoryRepository
    {
        private readonly DbSet<TicketHistory> _ticketHistory;
        public TicketsHistoryRepository(SystemTicketsContext context) : base(context)
        {
            _ticketHistory = _dbSet;
        }

        public async Task TrackChanges(Ticket ticket, Guid changeByUserId)
        {
            var changeGroupId = Guid.NewGuid();
            var entry = _context.Entry(ticket);
            foreach (var property in entry.Properties)
            {
                if (property.IsModified)
                {
                    string fieldName = property.Metadata.PropertyInfo?.Name ?? property.Metadata.Name;
                    var newTicketHistory = new TicketHistory
                    {
                        Ticket = ticket,
                        ChangedByUserId = changeByUserId,
                        ChangeGroupId = changeGroupId,
                        FieldName = fieldName,
                        OldValue = $"{property.OriginalValue}",
                        NewValue = $"{property.CurrentValue}"
                    };
                    await _ticketHistory.AddAsync(newTicketHistory);
                }
            }
        }

        public async Task<IEnumerable<TicketHistory>> GetTicketHistories(Guid ticketId)
            => await _ticketHistory.Where(t => t.TicketId == ticketId).ToListAsync();
    }
}
