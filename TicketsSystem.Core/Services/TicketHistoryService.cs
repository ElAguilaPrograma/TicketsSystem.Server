using FluentResults;
using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Core.DTOs.TicketsHistoryDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{
    public class TicketHistoryService : ITicketHistoryService
    {
        private readonly ITicketsHistoryRepository _ticketsHistoryRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ICurrentUserService _currentUserService;
        public TicketHistoryService(ITicketsHistoryRepository ticketsHistoryRepository, 
            ITicketsRepository ticketsRepository, 
            ICurrentUserService currentUserService)
        {
            _ticketsHistoryRepository = ticketsHistoryRepository;
            _ticketsRepository = ticketsRepository;
            _currentUserService = currentUserService;
        }
        public async Task<Result<IEnumerable<TicketHistoryGroupDto>>> GetTicketHistoryAsync(string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("The ticket id is not valid"));

            Guid ticketId = Guid.Parse(ticketIdStr);

            var ticket = await _ticketsRepository.GetById(ticketId);
            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket is does not exist"));

            if (_currentUserService.GetCurrentUserRole() == "User")
            {
                if (ticket.CreatedByUserId != _currentUserService.GetCurrentUserId())
                    return Result.Fail(new ForbiddenError("The information you are trying to access is not accessible to you."));
            }

            var ticketsRaw = await _ticketsHistoryRepository.GetTicketHistories(ticketId);

            var groupedHistory = ticketsRaw
                .GroupBy(t => t.ChangeGroupId)
                .OrderByDescending(g => g.First().ChangedAt)
                .Select(g => new TicketHistoryGroupDto
                {
                    ChangeGroupId = g.Key,
                    ChangedAt = g.First().ChangedAt,
                    ChangedByUserId = g.First().ChangedByUserId,
                    Changes = g.Select(t => new TicketHistoryReadDto
                    {
                        TicketId = t.TicketId,
                        ChangedByUserId = t.ChangedByUserId,
                        ChangeGroupId = t.ChangeGroupId,
                        FieldName = t.FieldName,
                        OldValue = t.OldValue,
                        NewValue = t.NewValue,
                        ChangedAt = t.ChangedAt
                    })
                });

            return Result.Ok(groupedHistory);
        }
    }
}
