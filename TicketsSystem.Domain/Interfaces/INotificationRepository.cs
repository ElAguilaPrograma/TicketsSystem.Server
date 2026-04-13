using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Domain.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotification(Guid userId);
    }
}
