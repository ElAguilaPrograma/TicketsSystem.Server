using FluentResults;
using TicketsSystem.Core.DTOs.TicketsCommentsDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;
using TicketsSystem.Core.Interfaces;
using Microsoft.AspNetCore.Localization;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using TicketsSystem.Core.DTOs.NotificationDTO;
using TicketsSystem.Core.Helpers.Mappers;
using TicketsSystem.Domain.Enums;

namespace TicketsSystem.Core.Services
{
    public class TicketCommentsService : ITicketCommetsService
    {
        private readonly ITicketCommentsRepository _ticketCommentsRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITicketHubService _ticketHubService;
        private readonly INotificationService _notificationService;
        public TicketCommentsService(
            ITicketCommentsRepository ticketCommentsRepository,
            ICurrentUserService currentUserService,
            ITicketsRepository ticketsRepository,
            IUnitOfWork unitOfWork,
            ITicketHubService ticketHubService,
            INotificationService notificationService)
        {
            _ticketCommentsRepository = ticketCommentsRepository;
            _currentUserService = currentUserService;
            _ticketsRepository = ticketsRepository;
            _unitOfWork = unitOfWork;
            _ticketHubService = ticketHubService;
            _notificationService = notificationService;
        }

        public async Task<Result<TicketsCreateComment>> CreateTicketCommentAsync(string ticketIdStr, TicketsCreateComment ticketsCreateComment)
        {
            if (string.IsNullOrWhiteSpace(ticketsCreateComment.Content))
                return Result.Fail(new BadRequestError("Content format is incorrect"));

            Guid ticketId = Guid.Parse(ticketIdStr);

            if (_currentUserService.GetCurrentUserRole() == "User")
                ticketsCreateComment.IsInternal = false;

            var ticket = await _ticketsRepository.GetTicketById(ticketId);
            var currentUserId = _currentUserService.GetCurrentUserId();
            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket was not found"));

            if (currentUserId != ticket.CreatedByUserId && 
                currentUserId != ticket.AssignedToUserId)
            {
                Console.WriteLine("Se esta ejecutando este bloque de codigo?");
                Console.WriteLine("Creador del ticket: " + ticket.CreatedByUserId);
                Console.WriteLine("Asignado a: " + ticket.AssignedToUserId);
                Console.WriteLine("Current userId: " + _currentUserService.GetCurrentUserId());
                return Result.Fail(new ForbiddenError("Only the user who created the ticket and the agent in charge can comment."));
            }

            var ticketComment = ticketsCreateComment.ToEntity(ticketId, currentUserId);

            await _ticketCommentsRepository.Create(ticketComment);
            await _unitOfWork.SaveChangesAsync();

            if (ticket != null)
            {
                var readComment = ticketComment.ToReadDto(await _currentUserService.GetCurrentUserName());
                var readTicket = ticket.ToReadDto();

                var newNotificationComment = new NotificationCreateDto
                {
                    UserId = ticketComment.UserId,
                    ContentId = ticketComment.TicketId,
                    Type = nameof(NotificationsTypes.CreateANewComment),
                    Message = "A new ticket comment was created.",
                    IsRead = false,
                    Ticket = readTicket,
                    TicketsReadComment = readComment
                };

                await _notificationService.CreateANotificationAsync(newNotificationComment);
            }

            return Result.Ok().WithSuccess(new CreatedSuccess("Comment created successfully."));
        }

        public async Task<Result<IEnumerable<TicketsReadComment>>> GetTicketCommentsAsync(string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("TicketId was bad formated"));

            Guid ticketId = Guid.Parse(ticketIdStr);

            if (!await _ticketsRepository.TicketExist(ticketId))
                return Result.Fail(new NotFoundError("The ticket it does not exist"));

            var ticketsComments = await _ticketCommentsRepository.GetTicketComments(ticketId);

            IEnumerable<TicketsReadComment> ticketsReadComments = ticketsComments.Select(tc => tc.ToReadDto());

            return Result.Ok(ticketsReadComments).WithSuccess(new OkSuccess("Comments retrieved successfully."));
        }

        public async Task<Result> UpdateTicketCommentAsync(TickersUpdateComment ticketsUpdateComment, string ticketCommentIdStr)
        {
            if (ticketsUpdateComment == null)
                return Result.Fail(new BadRequestError("Request body is required"));

            Guid ticketCommentId = Guid.Parse(ticketCommentIdStr);

            var ticketComment = await _ticketCommentsRepository.GetTicketCommentById(ticketCommentId);

            if (ticketComment == null)
                return Result.Fail(new NotFoundError("The ticket does not exit"));

            ticketComment.Content = ticketsUpdateComment.Content;
            ticketComment.IsInternal = ticketsUpdateComment.IsInternal;

            _ticketCommentsRepository.Update(ticketComment);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new OkSuccess("Comment updated successfully."));
        }
    }
}
