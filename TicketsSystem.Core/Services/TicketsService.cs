using FluentResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Core.Validations.TicketsValidations;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{


    public class TicketsService : ITicketsService
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGetUserRole _getUseRole;
        private readonly TicketsCreateValidator _ticketsCreateValidator;
        private readonly TicketsUpdateValidator _ticketsUpdateValidator;
        private readonly IUserRepository _userRepository;
        private readonly ITicketsHistoryRepository _ticketsHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        public TicketsService(ITicketsRepository ticketsRepository,
            ICurrentUserService currentUserService,
            TicketsCreateValidator ticketsCreateValidator,
            IGetUserRole getUserRole,
            TicketsUpdateValidator ticketsUpdateValidator,
            IUserRepository userRepository,
            ITicketsHistoryRepository ticketsHistoryRepository,
            IUnitOfWork unitOfWork)
        {
            _ticketsRepository = ticketsRepository;
            _currentUserService = currentUserService;
            _getUseRole = getUserRole;
            _ticketsCreateValidator = ticketsCreateValidator;
            _ticketsUpdateValidator = ticketsUpdateValidator;
            _userRepository = userRepository;
            _ticketsHistoryRepository = ticketsHistoryRepository;
            _unitOfWork = unitOfWork;
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
            string currentUserRole = _currentUserService.GetCurrentUserRole() ?? "";

            var tickets = await _ticketsRepository.GetCurrentUserTickets(currentUserId, currentUserRole);

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

            var tickets = await _ticketsRepository.GetTicketsByUserId(userId);

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

            await _ticketsRepository.Create(newTicket);

            var newTicketHistory = new TicketHistory
            {
                Ticket = newTicket,
                ChangedByUserId = _currentUserService.GetCurrentUserId(),
                FieldName = "Ticket Created"
            };

            await _ticketsHistoryRepository.Create(newTicketHistory);

            await _unitOfWork.SaveChangesAsync();

            return Result.Ok();
        }

        // Cuidado con las pruebas, revisar que el AssinedToUserId no sea un
        // Guid que no exista en la tabla User, en producción no debe de fallar asi
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

            if (ticketsUpdateDto.AssignedToUserId != null)
            {
                var user = await _userRepository.GetById(ticketsUpdateDto.AssignedToUserId.Value);

                if (user == null)
                    return Result.Fail(new NotFoundError("The agent you are trying to assign does not exist."));

                if (await _getUseRole.UserIsAgent(ticketsUpdateDto.AssignedToUserId.Value) == false)
                    return Result.Fail(new ForbiddenError("The user is not Agent"));

                ticket.AssignedToUserId = ticketsUpdateDto.AssignedToUserId;
            }

            _ticketsRepository.Update(ticket);

            await _unitOfWork.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> UpdateTicketPriority(TicketsUpdateDto ticketsUpdateDto, string ticketIdStr)
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

            _ticketsRepository.Update(ticket);
            await _unitOfWork.SaveChangesAsync();

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

            _ticketsRepository.Update(ticket);
            await _unitOfWork.SaveChangesAsync();

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

            _ticketsRepository.Update(ticket);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result<IEnumerable<TicketsReadDto>>> SearchTicketsAsync(
            string query, 
            int? statusValue = null, 
            int? priorityValue = null)
        {
            if (string.IsNullOrWhiteSpace(query) || query == null)
                return Result.Fail(new BadRequestError("Query format is not valid"));

            query = query.ToLower();
            var tickets = await _ticketsRepository.SearchTickets(query, statusValue, priorityValue);

            if (tickets == null)
                return Result.Fail(new NotFoundError("No tickets were found"));

            IEnumerable<TicketsReadDto> ticketsReadDtos = tickets.Select(t => new TicketsReadDto
            {
                TicketId = t.TicketId,
                Title = t.Title,
                Description = t.Description,
                StatusName = t.Status?.Name,
                PriorityName = t.Priority?.Name,
                AssignedToUser = t.AssignedToUser?.FullName ?? "To be defined",
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt
            });

            return Result.Ok(ticketsReadDtos);
        }

        public async Task<Result> CloseTicketsAsync(string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("The ticket id is not valid"));

            Guid ticketId = Guid.Parse(ticketIdStr);

            var ticket = await _ticketsRepository.GetById(ticketId);
            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket is does not exist"));

            if (ticket.StatusId == 4)
                return Result.Fail(new BadRequestError("The ticket is already close"));

            ticket.StatusId = 4;

            _ticketsRepository.Update(ticket);

            await _unitOfWork.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result> ReopenTicketsAsync(string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("The ticket id is not valid"));

            Guid ticketId = Guid.Parse(ticketIdStr);

            var ticket = await _ticketsRepository.GetById(ticketId);
            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket is does not exist"));

            if (ticket.StatusId != 4)
                return Result.Fail(new BadRequestError("The ticket is already open"));

            ticket.StatusId = 5;

            _ticketsRepository.Update(ticket);

            await _unitOfWork.SaveChangesAsync();

            return Result.Ok();
        }
    }
}
