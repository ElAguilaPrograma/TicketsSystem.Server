using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SystemTicketsContext _context;

        public UnitOfWork(SystemTicketsContext systemTicketsContext)
        {
            _context = systemTicketsContext;
        }

        public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
