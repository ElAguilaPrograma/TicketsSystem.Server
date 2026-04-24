using FluentResults;
using TicketsSystem.Core.DTOs.NotificationDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Helpers.Mappers;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Enums;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITicketHubService _ticketHubService;

        public NotificationService(
            INotificationRepository notificationRepository, 
            IUserRepository userRepository, 
            IUnitOfWork unitOfWork,
            ITicketHubService ticketHubService)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _ticketHubService = ticketHubService;
        }

        public async Task<Result<IEnumerable<NotificationReadDto>>> GetUserNotificationsAsync(string userIdStr)
        {
            if (string.IsNullOrEmpty(userIdStr))
                return Result.Fail(new BadRequestError("User id is requiered"));

            Guid userId = Guid.Parse(userIdStr);
            if (!await _userRepository.UserExist(userId))
                return Result.Fail(new NotFoundError("The user does not exist"));

            var notifications = await _notificationRepository.GetUserNotification(userId);

            var notificationsDto = notifications.Select(n => n.ToReadDto());
            return Result.Ok(notificationsDto);
        }

        public async Task<Result> CreateANotificationAsync(NotificationCreateDto notificationCreateDto)
        {
            var notification = notificationCreateDto.ToEntity();

            await _notificationRepository.Create(notification);
            await _unitOfWork.SaveChangesAsync();

            if (notificationCreateDto.Ticket != null)
            {
                if (notificationCreateDto.Type == nameof(NotificationsTypes.NewTicket))
                {
                    await _ticketHubService.NotifyTicketCreated(notificationCreateDto.Ticket);
                    await _ticketHubService.SendTicketToControlPanel(notificationCreateDto.Ticket);
                }
                else if (notificationCreateDto.Type == nameof(NotificationsTypes.UpdateTicket))
                {
                    await _ticketHubService.NotifyTicketStatusChanged(notificationCreateDto.Ticket, notificationCreateDto.UserId);
                }
                else if (notificationCreateDto.Type == nameof(NotificationsTypes.CreateANewComment))
                {
                    await _ticketHubService.NotifyTicketCommentCreated(notificationCreateDto.TicketsReadComment!, 
                        notificationCreateDto.UserId, notificationCreateDto.Ticket.AssignedToUserId);
                }
            }

            return Result.Ok();
        }

        public async Task<Result> ToggleNotificationReadStatusAsync(string notificationIdStr)
        {
            if (string.IsNullOrEmpty(notificationIdStr))
                return Result.Fail(new BadRequestError("Notification id is requiered"));

            Guid notificationId = Guid.Parse(notificationIdStr);
            var notification  = await _notificationRepository.GetById(notificationId);
            if (notification == null)
                return Result.Fail(new NotFoundError("The notification was not found"));

            notification.IsRead = true;
            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok();
        }
    }
}
