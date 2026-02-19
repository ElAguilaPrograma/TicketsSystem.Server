using FluentResults;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Validations.TicketsValidations;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

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
        Task<Result<IEnumerable<TicketsReadDto>>> SearchTicketsAsync(string query, int? statusId, int? priorityId);
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
        private readonly IUserRepository _userRepository;
        public TicketsService(ITicketsRepository ticketsRepository,
            ICurrentUserService currentUserService,
            TicketsCreateValidator ticketsCreateValidator,
            IGetUserRole getUserRole,
            TicketsUpdateValidator ticketsUpdateValidator,
            IUserRepository userRepository)
        {
            _ticketsRepository = ticketsRepository;
            _currentUserService = currentUserService;
            _getUseRole = getUserRole;
            _ticketsCreateValidator = ticketsCreateValidator;
            _ticketsUpdateValidator = ticketsUpdateValidator;
            _userRepository = userRepository;
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
                var user = await _userRepository.GetUserById(ticketsUpdateDto.AssignedToUserId.Value);

                if (user == null)
                    return Result.Fail(new NotFoundError("The agent you are trying to assign does not exist."));

                ticket.AssignedToUserId = ticketsUpdateDto.AssignedToUserId;
            }

                await _ticketsRepository.Update(ticket);

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

            await _ticketsRepository.Update(ticket);

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

            await _ticketsRepository.Update(ticket);

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

    }
}
