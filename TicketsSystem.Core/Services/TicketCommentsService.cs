using FluentResults;
using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Core.DTOs.TicketsCommentsDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{
    public interface ITicketCommetsService
    {
        Task<Result<TicketsCreateComment>> CreateTicketCommentAsync(string ticketIdStr, TicketsCreateComment ticketsCreateComment);
        Task<Result<IEnumerable<TicketsReadComment>>> GetTicketCommentsAsync(string ticketIdStr);
        Task<Result> UpdateTicketCommentAsync(TickersUpdateComment ticketsUpdateComment, string ticketCommentIdStr);
    }
    public class TicketCommentsService : ITicketCommetsService
    {
        private readonly ITicketCommentsRepository _ticketCommentsRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ICurrentUserService _currentUserService;
        public TicketCommentsService(ITicketCommentsRepository ticketCommentsRepository, ICurrentUserService currentUserService, ITicketsRepository ticketsRepository)
        {
            _ticketCommentsRepository = ticketCommentsRepository;
            _currentUserService = currentUserService;
            _ticketsRepository = ticketsRepository;
        }

        public async Task<Result<TicketsCreateComment>> CreateTicketCommentAsync(string ticketIdStr, TicketsCreateComment ticketsCreateComment)
        {
            if (string.IsNullOrWhiteSpace(ticketsCreateComment.Content))
                return Result.Fail(new BadRequestError("Content format is incorrect"));

            Guid ticketId = Guid.Parse(ticketIdStr);

            if (!await _ticketsRepository.TicketExist(ticketId))
                return Result.Fail(new NotFoundError("The ticket was not found"));

            if (_currentUserService.GetCurrentUserRole() == "User")
                ticketsCreateComment.IsInternal = false;

            var ticketComment = new TicketComment
            {
                TicketId = ticketId,
                UserId = _currentUserService.GetCurrentUserId(),
                Content = ticketsCreateComment.Content,
                IsInternal = ticketsCreateComment.IsInternal
            };

            await _ticketCommentsRepository.Create(ticketComment);

            return Result.Ok();
        }

        public async Task<Result<IEnumerable<TicketsReadComment>>> GetTicketCommentsAsync(string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("TicketId was bad formated"));

            Guid ticketId = Guid.Parse(ticketIdStr);

            if (!await _ticketsRepository.TicketExist(ticketId))
                return Result.Fail(new NotFoundError("The ticket it does not exist"));

            var ticketsComments = await _ticketCommentsRepository.GetTicketComments(ticketId);

            IEnumerable<TicketsReadComment> ticketsReadComments = ticketsComments.Select(tc => new TicketsReadComment
            {
                CommentId = tc.CommentId,
                UserId = tc.UserId,
                Content = tc.Content,
                IsInternal = tc.IsInternal
            });

            return Result.Ok(ticketsReadComments);
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

            await _ticketCommentsRepository.Update(ticketComment);

            return Result.Ok();
        }
    }
}
