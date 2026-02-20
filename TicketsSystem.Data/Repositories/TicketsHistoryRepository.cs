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
    }
}
