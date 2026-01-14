using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Validations;
using TicketsSystem.Core.Validations.TicketsValidations;
using TicketsSystem.Data.DTOs;
using TicketsSystem.Data.DTOs.TicketsDTO;
using TicketsSystem.Data.Repositories;
using TicketsSystem_Data;
using TicketsSystem_Data.Repositories;

namespace TicketsSystem.Core.Services
{
    public interface ITicketsService
    {
        Task<Result> AcceptTickets(string ticketIdStr);
        Task<Result> AssingTicketAsync(string userIdStr, string ticketIdSrt);
        Task<Result> CreateATicketAsync(TicketsCreateDto ticketsCreateDto);
        Task<Result<IEnumerable<TicketsReadDto>>> GetAllTicketsAsync();
        Task<Result<IEnumerable<TicketsReadDto>>> GetCurrentUserTicketsAsync();
        Task<Result<IEnumerable<TicketsReadDto>>> GetTicketsByUserIdAsync(string userIdStr);
        Task<Result> UpdateATicketInfoAsync(TicketsUpdateDto ticketsUpdateDto, string ticketIdStr);
        Task<Result> UpdateATicketInfoUserAsync([FromBody] TicketsUpdateDto ticketsUpdateDto, string ticketIdStr);
    }

    public class TicketsService : ITicketsService
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGetUserRole _getUseRole;
        private readonly TicketsCreateValidator _ticketsCreateValidator;
        private readonly TicketsUpdateValidator _ticketsUpdateValidator;
        public TicketsService(ITicketsRepository ticketsRepository, 
            ICurrentUserService currentUserService,
            TicketsCreateValidator ticketsCreateValidator,
            IGetUserRole getUserRole,
            TicketsUpdateValidator ticketsUpdateValidator)
        {
            _ticketsRepository = ticketsRepository;
            _currentUserService = currentUserService;
            _getUseRole = getUserRole;
            _ticketsCreateValidator = ticketsCreateValidator;
            _ticketsUpdateValidator = ticketsUpdateValidator;
        }

        public async Task<Result<IEnumerable<TicketsReadDto>>> GetAllTicketsAsync()
        {
            var tickets = await _ticketsRepository.GetAllTickets();

            IEnumerable<TicketsReadDto> ticketsDTOs = tickets.Select(t => new TicketsReadDto
            {
                TicketId = t.TicketId,
                Title = t.Title,
                Description = t.Description,
                StatusName = t.Status.Name,
                PriorityName = t.Priority.Name,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt
            });

            return Result.Ok(ticketsDTOs);
        }

        public async Task<Result<IEnumerable<TicketsReadDto>>> GetCurrentUserTicketsAsync()
        {
            Guid currentUserId = _currentUserService.GetCurrentUserId();

            var tickets = await _ticketsRepository.GetCurrentUserTickets(currentUserId);

            IEnumerable<TicketsReadDto> ticketsDTOs = tickets.Select(t => new TicketsReadDto
            {
                TicketId = t.TicketId,
                Title = t.Title,
                Description = t.Description,
                StatusName = t.Status.Name,
                PriorityName = t.Priority.Name,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt
            });

            return Result.Ok(ticketsDTOs);
        }

        public async Task<Result<IEnumerable<TicketsReadDto>>> GetTicketsByUserIdAsync(string userIdStr)
        {
            Guid userId = Guid.Parse(userIdStr);

            var tickets = await _ticketsRepository.GetCurrentUserTickets(userId);

            IEnumerable<TicketsReadDto> ticketsDTOs = tickets.Select(t => new TicketsReadDto
            {
                TicketId = t.TicketId,
                Title = t.Title,
                Description = t.Description,
                StatusName = t.Status.Name,
                PriorityName = t.Priority.Name,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt
            });

            return Result.Ok(ticketsDTOs);
        }

        public async Task<Result> CreateATicketAsync(TicketsCreateDto ticketsCreateDto)
        {
            if (ticketsCreateDto == null)
                return Result.Fail(new BadRequestError("Request body is requiered"));

            var validationResults = await _ticketsCreateValidator.ValidateAsync(ticketsCreateDto);
            if (!validationResults.IsValid)
            {
                var errorMessages = validationResults.Errors.Select(e => new BadRequestError(e.ErrorMessage));
                return Result.Fail(errorMessages);
            }

            Guid userId = _currentUserService.GetCurrentUserId();

            var newTicket = new Ticket
            {
                Title = ticketsCreateDto.Title,
                Description = ticketsCreateDto.Description,
                StatusId = 1,
                PriorityId = ticketsCreateDto.PriorityId,
                CreatedByUserId = userId
            };

            await _ticketsRepository.CreateTicket(newTicket);

            return Result.Ok();
        }

        public async Task<Result> UpdateATicketInfoAsync(TicketsUpdateDto ticketsUpdateDto, string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("Ticket id is requiered"));

            Guid ticketId = Guid.Parse(ticketIdStr);
            var ticket = await _ticketsRepository.GetTicketById(ticketId);

            if (ticket == null)
                return Result.Fail(new NotFoundError("Ticket not found"));

            if (ticketsUpdateDto == null)
                return Result.Fail(new BadRequestError("Request body is requiered"));

            var validationResults = await _ticketsUpdateValidator.ValidateAsync(ticketsUpdateDto);
            if (!validationResults.IsValid)
            {
                var errorMessages = validationResults.Errors.Select(e => new BadRequestError(e.ErrorMessage));
                return Result.Fail(errorMessages);
            }

            ticket.Title = ticketsUpdateDto.Title;
            ticket.Description = ticketsUpdateDto.Description;
            ticket.StatusId = ticketsUpdateDto.StatusId;
            ticket.PriorityId = ticketsUpdateDto.PriorityId;
            ticket.UpdatedAt = ticketsUpdateDto.UpdatedAt;
            ticket.AssignedToUserId = ticketsUpdateDto.AssignedToUserId;

            await _ticketsRepository.UpdateTicketInfo(ticket);

            return Result.Ok();
        }

        public async Task<Result> UpdateATicketInfoUserAsync(TicketsUpdateDto ticketsUpdateDto, string ticketIdStr)
        {
            if (ticketsUpdateDto == null)
                return Result.Fail(new BadRequestError("Request body is required"));

            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("Ticket id is required"));

            var validationResults = await _ticketsUpdateValidator.ValidateAsync(ticketsUpdateDto);
            if (!validationResults.IsValid)
            {
                var errorMessages = validationResults.Errors.Select(e => new BadRequestError(e.ErrorMessage));
                return Result.Fail(errorMessages);
            }

            Guid ticketId = Guid.Parse(ticketIdStr);
            var ticket = await _ticketsRepository.GetTicketById(ticketId);

            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket does not exist"));

            if (ticket.AssignedToUserId != null)
                return Result.Fail(new BadRequestError("The ticket was already accepted by an Agent"));

            ticket.PriorityId = ticketsUpdateDto.PriorityId;
            
            return Result.Ok();
        }
             
        public async Task<Result> AssingTicketAsync(string userIdStr, string ticketIdSrt)
        {
            if (string.IsNullOrWhiteSpace(ticketIdSrt) || ticketIdSrt == null)
                return Result.Fail(new BadRequestError("The ticket id is not valid"));

            if (string.IsNullOrWhiteSpace(userIdStr) || userIdStr == null)
                return Result.Fail(new BadRequestError("The userId is not valid"));

            Guid userId = Guid.Parse(userIdStr);
            Guid ticketId = Guid.Parse(ticketIdSrt);

            if (await _getUseRole.UserIsAgent(userId) == false)
                return Result.Fail(new ForbiddenError("The user is not Agent"));

            var ticket = await _ticketsRepository.GetTicketById(ticketId);

            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket does not exist"));

            ticket.AssignedToUserId = userId;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketsRepository.AssingTicket(ticket);

            return Result.Ok();
        }

        public async Task<Result> AcceptTickets(string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr) || ticketIdStr == null)
                return Result.Fail(new BadRequestError("The ticket id is not valid"));

            Guid userId = _currentUserService.GetCurrentUserId();
            Guid ticketId = Guid.Parse(ticketIdStr);

            if (await _getUseRole.UserIsAgent(userId) == false)
                return Result.Fail(new ForbiddenError("The user is not Agent"));

            var ticket = await _ticketsRepository.GetTicketById(ticketId);

            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket does not exist"));

            ticket.AssignedToUserId = userId;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketsRepository.AssingTicket(ticket);

            return Result.Ok();
        }

    }
}
