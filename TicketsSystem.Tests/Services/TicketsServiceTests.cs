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

public class TicketsServiceTests
{
    private readonly Mock<ITicketsRepository> _ticketsRepository = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IGetUserRole> _getUserRole = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ITicketsHistoryRepository> _ticketsHistoryRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<INotificationService> _notificationService = new();

    private TicketsService BuildSut()
    {
        return new TicketsService(
            _ticketsRepository.Object,
            _currentUserService.Object,
            _getUserRole.Object,
            _userRepository.Object,
            _ticketsHistoryRepository.Object,
            _unitOfWork.Object,
            _notificationService.Object);
    }

    [Fact]
    public async Task GetAllTicketsWithFiltersAsync_ReturnsForbidden_ForUserWithoutSelfFilters()
    {
        var filter = new GetAllTicketsFilterDto
        {
            Page = 1,
            PageSize = 10,
            CurrentUserOnly = false,
            AssignedToMeOnly = false
        };
        _currentUserService.Setup(x => x.GetCurrentUserRole()).Returns("User");
        var sut = BuildSut();

        var result = await sut.GetAllTicketsWithFiltersAsync(filter);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ForbiddenError);
        _ticketsRepository.Verify(x => x.GetAllTicketsPaginatedWithFilters(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<Guid?>(),
            It.IsAny<bool?>(),
            It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task CreateATicketAsync_CreatesTicketHistoryAndNotification_WhenTicketCanBeLoaded()
    {
        var currentUserId = Guid.NewGuid();
        var createdTicketId = Guid.NewGuid();
        var input = new TicketsCreateDto
        {
            Title = "Email down",
            Description = "SMTP service unavailable",
            PriorityId = (int)TicketsPriorityValue.Medium
        };
        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(currentUserId);

        _ticketsRepository
            .Setup(x => x.Create(It.IsAny<Ticket>()))
            .Callback<Ticket>(t => t.TicketId = createdTicketId)
            .Returns(Task.CompletedTask);

        _ticketsRepository
            .Setup(x => x.GetTicketById(createdTicketId))
            .ReturnsAsync(BuildTicket(createdTicketId, currentUserId, (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Medium));

        _notificationService
            .Setup(x => x.CreateANotificationAsync(It.IsAny<NotificationCreateDto>()))
            .ReturnsAsync(FluentResults.Result.Ok());

        var sut = BuildSut();

        var result = await sut.CreateATicketAsync(input);

        result.IsSuccess.Should().BeTrue();
        _ticketsRepository.Verify(x => x.Create(It.Is<Ticket>(t =>
            t.Title == input.Title &&
            t.Description == input.Description &&
            t.CreatedByUserId == currentUserId &&
            t.StatusId == (int)TicketsStatusValue.Open)), Times.Once);
        _ticketsHistoryRepository.Verify(x => x.Create(It.Is<TicketHistory>(h =>
            h.ChangedByUserId == currentUserId &&
            h.FieldName == "Ticket Created")), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _notificationService.Verify(x => x.CreateANotificationAsync(It.IsAny<NotificationCreateDto>()), Times.Once);
    }

    [Fact]
    public async Task UpdateATicketInfoAsync_ReturnsBadRequest_WhenTicketIdIsEmpty()
    {
        var input = new TicketsUpdateDto
        {
            Title = "Updated",
            Description = "Updated description",
            StatusId = (int)TicketsStatusValue.InProgress,
            PriorityId = (int)TicketsPriorityValue.High
        };
        var sut = BuildSut();

        var result = await sut.UpdateATicketInfoAsync(input, string.Empty);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
    }

    [Fact]
    public async Task UpdateATicketInfoAsync_ReturnsBadRequest_WhenAgentIsNotAssignedToTicket()
    {
        var currentUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var existingTicket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Low);
        existingTicket.AssignedToUserId = Guid.NewGuid();

        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(existingTicket);
        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(currentUserId);
        _currentUserService.Setup(x => x.GetCurrentUserRole()).Returns("Agent");

        var input = new TicketsUpdateDto
        {
            Title = "Updated",
            Description = "Updated description",
            StatusId = (int)TicketsStatusValue.InProgress,
            PriorityId = (int)TicketsPriorityValue.Medium
        };
        var sut = BuildSut();

        var result = await sut.UpdateATicketInfoAsync(input, ticketId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateATicketInfoAsync_ReturnsNotFound_WhenAssignedAgentDoesNotExist()
    {
        var currentUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var existingTicket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Low);

        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(existingTicket);
        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(currentUserId);
        _currentUserService.Setup(x => x.GetCurrentUserRole()).Returns("Admin");
        _userRepository.Setup(x => x.GetById(assigneeId)).ReturnsAsync((User?)null);

        var input = new TicketsUpdateDto
        {
            Title = "Updated",
            Description = "Updated description",
            StatusId = (int)TicketsStatusValue.InProgress,
            PriorityId = (int)TicketsPriorityValue.High,
            AssignedToUserId = assigneeId
        };
        var sut = BuildSut();

        var result = await sut.UpdateATicketInfoAsync(input, ticketId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is NotFoundError);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AssingTicketAsync_ReturnsForbidden_WhenTargetUserIsNotAgent()
    {
        var targetUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        _getUserRole.Setup(x => x.UserIsAgent(targetUserId)).ReturnsAsync(false);

        var sut = BuildSut();

        var result = await sut.AssingTicketAsync(targetUserId.ToString(), ticketId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ForbiddenError);
        _ticketsRepository.Verify(x => x.GetTicketById(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task AssingTicketAsync_AssignsTicketAndPersists_WhenInputIsValid()
    {
        var actingUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Low);

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(actingUserId);
        _getUserRole.Setup(x => x.UserIsAgent(targetUserId)).ReturnsAsync(true);
        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(ticket);

        var sut = BuildSut();

        var result = await sut.AssingTicketAsync(targetUserId.ToString(), ticketId.ToString());

        result.IsSuccess.Should().BeTrue();
        ticket.AssignedToUserId.Should().Be(targetUserId);
        _ticketsRepository.Verify(x => x.Update(ticket), Times.Once);
        _ticketsHistoryRepository.Verify(x => x.TrackChanges(ticket, actingUserId), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CloseTicketsAsync_ReturnsBadRequest_WhenTicketIdIsEmpty()
    {
        var sut = BuildSut();

        var result = await sut.CloseTicketsAsync(string.Empty);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
    }

    [Fact]
    public async Task CloseTicketsAsync_ReturnsBadRequest_WhenTicketIsAlreadyClosed()
    {
        var ticketId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Closed, (int)TicketsPriorityValue.Low);
        _ticketsRepository.Setup(x => x.GetById(ticketId)).ReturnsAsync(ticket);
        var sut = BuildSut();

        var result = await sut.CloseTicketsAsync(ticketId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
        _ticketsRepository.Verify(x => x.Update(It.IsAny<Ticket>()), Times.Never);
    }

    [Fact]
    public async Task CloseTicketsAsync_ClosesTicketAndCreatesNotification_WhenTicketIsOpen()
    {
        var actingUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Medium);
        var ticketAfterClose = BuildTicket(ticketId, ticket.CreatedByUserId, (int)TicketsStatusValue.Closed, (int)TicketsPriorityValue.Medium);

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(actingUserId);
        _ticketsRepository.Setup(x => x.GetById(ticketId)).ReturnsAsync(ticket);
        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(ticketAfterClose);
        _notificationService
            .Setup(x => x.CreateANotificationAsync(It.IsAny<NotificationCreateDto>()))
            .ReturnsAsync(FluentResults.Result.Ok());

        var sut = BuildSut();

        var result = await sut.CloseTicketsAsync(ticketId.ToString());

        result.IsSuccess.Should().BeTrue();
        ticket.StatusId.Should().Be((int)TicketsStatusValue.Closed);
        ticket.ClosedAt.Should().NotBeNull();
        _ticketsRepository.Verify(x => x.Update(ticket), Times.Once);
        _ticketsHistoryRepository.Verify(x => x.TrackChanges(ticket, actingUserId), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _notificationService.Verify(x => x.CreateANotificationAsync(It.Is<NotificationCreateDto>(n =>
            n.Type == nameof(NotificationsTypes.UpdateTicket) &&
            n.UserId == ticket.CreatedByUserId)), Times.Once);
    }

    [Fact]
    public async Task ReopenTicketsAsync_ReturnsBadRequest_WhenTicketIsNotClosed()
    {
        var ticketId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Low);
        _ticketsRepository.Setup(x => x.GetById(ticketId)).ReturnsAsync(ticket);
        var sut = BuildSut();

        var result = await sut.ReopenTicketsAsync(ticketId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
        _ticketsRepository.Verify(x => x.Update(It.IsAny<Ticket>()), Times.Never);
    }

    [Fact]
    public async Task ReopenTicketsAsync_ReopensTicketAndCreatesNotification_WhenTicketIsClosed()
    {
        var actingUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Closed, (int)TicketsPriorityValue.High);
        ticket.ClosedAt = DateTime.UtcNow;
        var ticketAfterReopen = BuildTicket(ticketId, ticket.CreatedByUserId, (int)TicketsStatusValue.Reopened, (int)TicketsPriorityValue.High);

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(actingUserId);
        _ticketsRepository.Setup(x => x.GetById(ticketId)).ReturnsAsync(ticket);
        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(ticketAfterReopen);
        _notificationService
            .Setup(x => x.CreateANotificationAsync(It.IsAny<NotificationCreateDto>()))
            .ReturnsAsync(FluentResults.Result.Ok());

        var sut = BuildSut();

        var result = await sut.ReopenTicketsAsync(ticketId.ToString());

        result.IsSuccess.Should().BeTrue();
        ticket.StatusId.Should().Be((int)TicketsStatusValue.Reopened);
        ticket.ClosedAt.Should().BeNull();
        _ticketsRepository.Verify(x => x.Update(ticket), Times.Once);
        _ticketsHistoryRepository.Verify(x => x.TrackChanges(ticket, actingUserId), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _notificationService.Verify(x => x.CreateANotificationAsync(It.Is<NotificationCreateDto>(n =>
            n.Type == nameof(NotificationsTypes.UpdateTicket) &&
            n.UserId == ticket.CreatedByUserId)), Times.Once);
    }

    [Fact]
    public async Task UpdateTicketUser_ReturnsBadRequest_WhenCurrentUserIsNotTicketOwner()
    {
        var currentUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticketOwnerId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, ticketOwnerId, (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Medium);

        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(ticket);
        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(currentUserId);

        var input = new TicketsUpdateDto
        {
            Title = "Edited title",
            Description = "Edited description",
            StatusId = ticket.StatusId,
            PriorityId = (int)TicketsPriorityValue.High,
            AssignedToUserId = ticket.AssignedToUserId
        };
        var sut = BuildSut();

        var result = await sut.UpdateTicketUser(input, ticketId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateTicketUser_ReturnsForbidden_WhenUserAttemptsToChangeStatusOrAssignment()
    {
        var ownerId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, ownerId, (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Low);

        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(ticket);
        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(ownerId);

        var input = new TicketsUpdateDto
        {
            Title = "Edited title",
            Description = "Edited description",
            StatusId = (int)TicketsStatusValue.InProgress,
            PriorityId = (int)TicketsPriorityValue.High,
            AssignedToUserId = ticket.AssignedToUserId
        };
        var sut = BuildSut();

        var result = await sut.UpdateTicketUser(input, ticketId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ForbiddenError);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateTicketUser_UpdatesAllowedFields_WhenOwnerSendsValidChanges()
    {
        var ownerId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, ownerId, (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Low);

        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(ticket);
        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(ownerId);

        var input = new TicketsUpdateDto
        {
            Title = "Edited title",
            Description = "Edited description",
            StatusId = ticket.StatusId,
            PriorityId = (int)TicketsPriorityValue.Critical,
            AssignedToUserId = ticket.AssignedToUserId
        };
        var sut = BuildSut();

        var result = await sut.UpdateTicketUser(input, ticketId.ToString());

        result.IsSuccess.Should().BeTrue();
        ticket.Title.Should().Be(input.Title);
        ticket.Description.Should().Be(input.Description);
        ticket.PriorityId.Should().Be((int)TicketsPriorityValue.Critical);
        _ticketsRepository.Verify(x => x.Update(ticket), Times.Once);
        _ticketsHistoryRepository.Verify(x => x.TrackChanges(ticket, ownerId), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateATicketInfoAsync_CreatesNotification_WhenStatusChanges()
    {
        var currentUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var existingTicket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Medium);
        var updatedTicket = BuildTicket(ticketId, existingTicket.CreatedByUserId, (int)TicketsStatusValue.Closed, (int)TicketsPriorityValue.Medium);
        updatedTicket.Title = "Updated title";

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(currentUserId);
        _currentUserService.Setup(x => x.GetCurrentUserRole()).Returns("Admin");
        _ticketsRepository
            .SetupSequence(x => x.GetTicketById(ticketId))
            .ReturnsAsync(existingTicket)
            .ReturnsAsync(updatedTicket);
        _notificationService
            .Setup(x => x.CreateANotificationAsync(It.IsAny<NotificationCreateDto>()))
            .ReturnsAsync(FluentResults.Result.Ok());

        var input = new TicketsUpdateDto
        {
            Title = "Updated title",
            Description = "Updated description",
            StatusId = (int)TicketsStatusValue.Closed,
            PriorityId = (int)TicketsPriorityValue.Medium,
            AssignedToUserId = null
        };
        var sut = BuildSut();

        var result = await sut.UpdateATicketInfoAsync(input, ticketId.ToString());

        result.IsSuccess.Should().BeTrue();
        _ticketsHistoryRepository.Verify(x => x.TrackChanges(existingTicket, currentUserId), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _notificationService.Verify(x => x.CreateANotificationAsync(It.Is<NotificationCreateDto>(n =>
            n.Type == nameof(NotificationsTypes.UpdateTicket) &&
            n.UserId == existingTicket.CreatedByUserId &&
            n.Ticket != null &&
            n.Ticket.TicketId == ticketId)), Times.Once);
    }

    [Fact]
    public async Task UpdateATicketInfoAsync_DoesNotCreateNotification_WhenStatusDoesNotChange()
    {
        var currentUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var existingTicket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Low);

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(currentUserId);
        _currentUserService.Setup(x => x.GetCurrentUserRole()).Returns("Admin");
        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(existingTicket);

        var input = new TicketsUpdateDto
        {
            Title = "Still open",
            Description = "No status change",
            StatusId = (int)TicketsStatusValue.Open,
            PriorityId = (int)TicketsPriorityValue.High,
            AssignedToUserId = null
        };
        var sut = BuildSut();

        var result = await sut.UpdateATicketInfoAsync(input, ticketId.ToString());

        result.IsSuccess.Should().BeTrue();
        _notificationService.Verify(x => x.CreateANotificationAsync(It.IsAny<NotificationCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task AbandonATicketAsync_ReturnsBadRequest_WhenTicketIdIsEmpty()
    {
        var sut = BuildSut();

        var result = await sut.AbandonATicketAsync(string.Empty);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
    }

    [Fact]
    public async Task AbandonATicketAsync_ReturnsNotFound_WhenTicketDoesNotExist()
    {
        var ticketId = Guid.NewGuid();
        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync((Ticket?)null);
        var sut = BuildSut();

        var result = await sut.AbandonATicketAsync(ticketId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is NotFoundError);
    }

    [Fact]
    public async Task AbandonATicketAsync_UnassignsTicketAndPersists_WhenTicketExists()
    {
        var actingUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.InProgress, (int)TicketsPriorityValue.High);
        ticket.AssignedToUserId = Guid.NewGuid();

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(actingUserId);
        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(ticket);
        var sut = BuildSut();

        var result = await sut.AbandonATicketAsync(ticketId.ToString());

        result.IsSuccess.Should().BeTrue();
        ticket.AssignedToUserId.Should().BeNull();
        _ticketsRepository.Verify(x => x.Update(ticket), Times.Once);
        _ticketsHistoryRepository.Verify(x => x.TrackChanges(ticket, actingUserId), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AcceptTickets_AssignsCurrentUser_WhenInputIsValid()
    {
        var currentUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = BuildTicket(ticketId, Guid.NewGuid(), (int)TicketsStatusValue.Open, (int)TicketsPriorityValue.Low);

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(currentUserId);
        _getUserRole.Setup(x => x.UserIsAgent(currentUserId)).ReturnsAsync(true);
        _ticketsRepository.Setup(x => x.GetTicketById(ticketId)).ReturnsAsync(ticket);
        var sut = BuildSut();

        var result = await sut.AcceptTickets(ticketId.ToString());

        result.IsSuccess.Should().BeTrue();
        ticket.AssignedToUserId.Should().Be(currentUserId);
        _ticketsRepository.Verify(x => x.Update(ticket), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUserTicketsCountAsync_MapsRepositorySummary_ToDto()
    {
        var currentUserId = Guid.NewGuid();
        var summary = new Dictionary<int, int>
        {
            [(int)TicketsStatusValue.Open] = 4,
            [(int)TicketsStatusValue.Closed] = 3,
            [(int)TicketsStatusValue.Reopened] = 1
        };

        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(currentUserId);
        _currentUserService.Setup(x => x.GetCurrentUserRole()).Returns("Agent");
        _ticketsRepository.Setup(x => x.GetTicketsCountSummary(currentUserId, "Agent")).ReturnsAsync(summary);
        var sut = BuildSut();

        var result = await sut.GetCurrentUserTicketsCountAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalTickets.Should().Be(8);
        result.Value.TicketsOpen.Should().Be(4);
        result.Value.TicketsClosed.Should().Be(3);
        result.Value.TicketsReopen.Should().Be(1);
    }

    [Fact]
    public async Task GetTodaysTicketsCountAsync_ReturnsCount_FromRepository()
    {
        _ticketsRepository.Setup(x => x.GetTodaysTicketsCount()).ReturnsAsync(6);
        var sut = BuildSut();

        var result = await sut.GetTodaysTicketsCountAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(6);
        _ticketsRepository.Verify(x => x.GetTodaysTicketsCount(), Times.Once);
    }

    [Fact]
    public async Task AssingTicketAsync_ThrowsFormatException_WhenUserIdIsInvalidGuid()
    {
        var sut = BuildSut();

        var action = async () => await sut.AssingTicketAsync("bad-guid", Guid.NewGuid().ToString());

        await action.Should().ThrowAsync<FormatException>();
    }

    [Fact]
    public async Task AssingTicketAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid()
    {
        var sut = BuildSut();

        var action = async () => await sut.AssingTicketAsync(Guid.NewGuid().ToString(), "bad-guid");

        await action.Should().ThrowAsync<FormatException>();
    }

    [Fact]
    public async Task UpdateATicketInfoAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid()
    {
        var input = new TicketsUpdateDto
        {
            Title = "Title",
            Description = "Description",
            StatusId = (int)TicketsStatusValue.Open,
            PriorityId = (int)TicketsPriorityValue.Low
        };
        var sut = BuildSut();

        var action = async () => await sut.UpdateATicketInfoAsync(input, "bad-guid");

        await action.Should().ThrowAsync<FormatException>();
    }

    [Fact]
    public async Task CloseTicketsAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid()
    {
        var sut = BuildSut();

        var action = async () => await sut.CloseTicketsAsync("bad-guid");

        await action.Should().ThrowAsync<FormatException>();
    }

    [Fact]
    public async Task ReopenTicketsAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid()
    {
        var sut = BuildSut();

        var action = async () => await sut.ReopenTicketsAsync("bad-guid");

        await action.Should().ThrowAsync<FormatException>();
    }

    [Fact]
    public async Task GetTicketByIdAsync_ThrowsFormatException_WhenTicketIdIsInvalidGuid()
    {
        var sut = BuildSut();

        var action = async () => await sut.GetTicketByIdAsync("bad-guid");

        await action.Should().ThrowAsync<FormatException>();
    }

    private static Ticket BuildTicket(Guid ticketId, Guid createdByUserId, int statusId, int priorityId)
    {
        return new Ticket
        {
            TicketId = ticketId,
            Title = "Sample ticket",
            Description = "Sample description",
            StatusId = statusId,
            PriorityId = priorityId,
            CreatedByUserId = createdByUserId,
            CreatedByUser = new User
            {
                UserId = createdByUserId,
                FullName = "Creator User",
                Email = "creator@test.com",
                PasswordHash = "hash",
                Role = "User"
            },
            Status = new TicketStatus { StatusId = statusId, Name = "Open" },
            Priority = new TicketPriority { PriorityId = priorityId, Name = "Medium", Level = 2 }
        };
    }
}
