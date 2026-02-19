using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly SystemTicketsContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(SystemTicketsContext systemTicketsContext)
        {
            _context = systemTicketsContext;
            _dbSet = systemTicketsContext.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll() => await _dbSet.ToListAsync();
        public async Task<T?> GetById(Guid id) => await _dbSet.FindAsync(id);

        public async Task Create(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _context.Update(entity);
        }
    }
}
