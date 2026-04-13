using FluentResults;
using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Core.DTOs.NotificationDTO;

namespace TicketsSystem.Core.Interfaces
{
    public interface INotificationService
    {
        Task<Result> CreateANotificationAsync(NotificationCreateDto notificationCreateDto);
        Task<Result<IEnumerable<NotificationReadDto>>> GetUserNotificationsAsync(string userIdStr);
        Task<Result> ToggleNotificationReadStatusAsync(string notificationIdStr);
    }
}
