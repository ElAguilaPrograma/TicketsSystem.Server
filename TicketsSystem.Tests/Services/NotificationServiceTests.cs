using FluentAssertions;
using Moq;
using TicketsSystem.Core.DTOs.NotificationDTO;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Core.Services;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Enums;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ITicketHubService> _ticketHubService = new();

    private NotificationService BuildSut()
    {
        return new NotificationService(
            _notificationRepository.Object,
            _userRepository.Object,
            _unitOfWork.Object,
            _ticketHubService.Object);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsBadRequest_WhenUserIdIsEmpty()
    {
        var sut = BuildSut();

        var result = await sut.GetUserNotificationsAsync(string.Empty);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userRepository.Setup(x => x.UserExist(userId)).ReturnsAsync(false);
        var sut = BuildSut();

        var result = await sut.GetUserNotificationsAsync(userId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is NotFoundError);
        _notificationRepository.Verify(x => x.GetUserNotification(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CreateANotificationAsync_PersistsAndBroadcasts_WhenTypeIsNewTicket()
    {
        var userId = Guid.NewGuid();
        var ticket = new TicketsReadDto
        {
            TicketId = Guid.NewGuid(),
            Title = "Printer not working",
            Description = "The office printer is jammed",
            StatusId = 1,
            PriorityId = 2,
            CreatedByUserId = userId,
            CreatedByUser = "User Test",
            CreatedAt = DateTime.UtcNow
        };
        var input = new NotificationCreateDto
        {
            UserId = userId,
            Type = nameof(NotificationsTypes.NewTicket),
            Message = "Ticket created",
            IsRead = false,
            Ticket = ticket
        };

        var sut = BuildSut();

        var result = await sut.CreateANotificationAsync(input);

        result.IsSuccess.Should().BeTrue();
        _notificationRepository.Verify(x => x.Create(It.Is<Notification>(n =>
            n.UserId == input.UserId &&
            n.Type == input.Type &&
            n.Message == input.Message &&
            n.IsRead == input.IsRead)), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _ticketHubService.Verify(x => x.NotifyTicketCreated(ticket), Times.Once);
    }

    [Fact]
    public async Task ToggleNotificationReadStatusAsync_UpdatesAndSaves_WhenNotificationExists()
    {
        var notificationId = Guid.NewGuid();
        var notification = new Notification
        {
            NotificationId = notificationId,
            UserId = Guid.NewGuid(),
            Type = nameof(NotificationsTypes.NewTicket),
            Message = "Ticket updated",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _notificationRepository.Setup(x => x.GetById(notificationId)).ReturnsAsync(notification);
        var sut = BuildSut();

        var result = await sut.ToggleNotificationReadStatusAsync(notificationId.ToString());

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _notificationRepository.Verify(x => x.Update(notification), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
