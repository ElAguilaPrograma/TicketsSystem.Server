using Microsoft.EntityFrameworkCore;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data.Repositories
{
    public class NotificationRepository: GenericRepository<Notification>, INotificationRepository
    {
        private readonly DbSet<Notification> _notifications;
        public NotificationRepository(SystemTicketsContext context): base (context)
        {
            _notifications = _dbSet;
        }

        public async Task<IEnumerable<Notification>> GetUserNotification(Guid userId)
        {
            var query = _notifications.Include(n => n.User).AsQueryable();
            query = query.Where(n => n.UserId == userId);
            var notifications = await query.ToListAsync();
            return notifications;
        }
    }
}
