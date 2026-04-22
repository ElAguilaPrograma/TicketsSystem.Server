using TicketsSystem.Core.DTOs.NotificationDTO;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Core.Helpers.Mappers;

public static class NotificationMappers
{
    public static Notification ToEntity(this NotificationCreateDto dto) => new()
    {
        UserId = dto.UserId,
        ContentId = dto.ContentId,
        Type = dto.Type,
        Message = dto.Message,
        IsRead = dto.IsRead,
        CreatedAt = DateTime.UtcNow
    };

    public static NotificationReadDto ToReadDto(this Notification notification) => new()
    {
        NotificationId = notification.NotificationId,
        UserId = notification.UserId,
        ContentId = notification.ContentId,
        Type = notification.Type,
        Message = notification.Message,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt
    };
}
