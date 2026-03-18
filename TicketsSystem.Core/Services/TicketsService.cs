using FluentResults;
using TicketsSystem.Core.DTOs.PaginationDTO;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Enums;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{
    public class TicketsService : ITicketsService
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGetUserRole _getUseRole;
        private readonly IUserRepository _userRepository;
        private readonly ITicketsHistoryRepository _ticketsHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITicketHubService _ticketHubService;

        public TicketsService(ITicketsRepository ticketsRepository,
            ICurrentUserService currentUserService,
            IGetUserRole getUserRole,
            IUserRepository userRepository,
            ITicketsHistoryRepository ticketsHistoryRepository,
            IUnitOfWork unitOfWork,
            ITicketHubService ticketHubService)
        {
            _ticketsRepository = ticketsRepository;
            _currentUserService = currentUserService;
            _getUseRole = getUserRole;
            _userRepository = userRepository;
            _ticketsHistoryRepository = ticketsHistoryRepository;
            _unitOfWork = unitOfWork;
            _ticketHubService = ticketHubService;
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
                AssignedToUser = t.AssignedToUser?.FullName ?? "To be defined",
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt
            });

            return Result.Ok(ticketsDTOs).WithSuccess(new OkSuccess("Tickets retrieved successfully."));
        }

        public async Task<Result<PagedResult<TicketsReadDto>>> GetAllTicketsWithFiltersAsync(GetAllTicketsFilterDto filterDto)
        {
            var validStatusValues = Enum.GetNames<TicketsStatusValue>().Append("All").ToArray();
            var validPriorityValues = Enum.GetNames<TicketsPriorityValue>().Append("All").ToArray();

            if (!validStatusValues.Contains(filterDto.Status))
                return Result.Fail(new BadRequestError($"Invalid status value. Valid status are: {string.Join(", ", validStatusValues)}"));
            if (!validPriorityValues.Contains(filterDto.Priority))
                return Result.Fail(new BadRequestError($"Invalid priority value. Valid priority are: {string.Join(", ", validPriorityValues)}"));
            if (filterDto.Month != null)
            {
                if (filterDto.Month <= 0 || filterDto.Month > 12)
                    return Result.Fail(new BadRequestError("Invalid month value"));
            }

            var (tickets, totalCount) = await _ticketsRepository.GetAllTicketsPaginatedWithFilters(
                filterDto.Page,
                filterDto.PageSize,
                filterDto.Status,
                filterDto.Priority,
                filterDto.QuerySearch,
                filterDto.Month,
                filterDto.Year);

            var ticketsDTOs = tickets.Select(t => new TicketsReadDto
            {
                TicketId = t.TicketId,
                Title = t.Title,
                Description = t.Description,
                StatusName = t.Status.Name,
                PriorityName = t.Priority.Name,
                AssignedToUser = t.AssignedToUser?.FullName,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt
            });

            var result = new PagedResult<TicketsReadDto>
            {
                Data = ticketsDTOs,
                TotalCount = totalCount,
                Page = filterDto.Page,
                PageSize = filterDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filterDto.PageSize)
            };

            return Result.Ok(result);
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
                AssignedToUser = t.AssignedToUser?.FullName ?? "To be defined",
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt
            });

            return Result.Ok(ticketsDTOs).WithSuccess(new OkSuccess("User tickets retrieved successfully."));
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
                AssignedToUser = t.AssignedToUser?.FullName ?? "To be defined",
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ClosedAt = t.ClosedAt
            });

            return Result.Ok(ticketsDTOs).WithSuccess(new OkSuccess("User tickets retrieved successfully."));
        }

        public async Task<Result> CreateATicketAsync(TicketsCreateDto ticketsCreateDto)
        {

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
                ChangeGroupId = Guid.NewGuid(),
                FieldName = "Ticket Created"
            };

            await _ticketsHistoryRepository.Create(newTicketHistory);

            await _unitOfWork.SaveChangesAsync();

            // Notify via SignalR
            var ticketWithData = await _ticketsRepository.GetTicketById(newTicket.TicketId);
            if (ticketWithData != null)
            {
                var ticketReadDto = new TicketsReadDto
                {
                    TicketId = ticketWithData.TicketId,
                    Title = ticketWithData.Title,
                    Description = ticketWithData.Description,
                    StatusName = ticketWithData.Status?.Name,
                    PriorityName = ticketWithData.Priority?.Name,
                    AssignedToUser = ticketWithData.AssignedToUser?.FullName ?? "To be defined",
                    CreatedAt = ticketWithData.CreatedAt,
                    UpdatedAt = ticketWithData.UpdatedAt,
                    ClosedAt = ticketWithData.ClosedAt
                };
                await _ticketHubService.NotifyTicketCreated(ticketReadDto);
            }

            return Result.Ok().WithSuccess(new CreatedSuccess("Ticket created successfully."));
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



            int originalStatusId = ticket.StatusId;

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

            if (ticketsUpdateDto.StatusId == 4)
                ticket.ClosedAt = DateTime.UtcNow;

            _ticketsRepository.Update(ticket);
            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            if (ticketsUpdateDto.StatusId != originalStatusId)
            {
                var updatedTicket = await _ticketsRepository.GetTicketById(ticketId);
                if (updatedTicket != null)
                {
                    var newTicketReadDto = new TicketsReadDto
                    {
                        TicketId = updatedTicket.TicketId,
                        Title = updatedTicket.Title,
                        Description = updatedTicket.Description,
                        StatusName = updatedTicket.Status?.Name,
                        PriorityName = updatedTicket.Priority?.Name,
                        AssignedToUser = updatedTicket.AssignedToUser?.FullName ?? "To be defined",
                        CreatedAt = updatedTicket.CreatedAt,
                        UpdatedAt = updatedTicket.UpdatedAt,
                        ClosedAt = updatedTicket.ClosedAt
                    };

                    await _ticketHubService.NotifyTicketStatusChanged(newTicketReadDto, updatedTicket.CreatedByUserId);
                }
            }

            return Result.Ok().WithSuccess(new OkSuccess("Ticket updated successfully."));
        }

        public async Task<Result> UpdateTicketPriority(TicketsUpdateDto ticketsUpdateDto, string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("Ticket id is required"));

            Guid ticketId = Guid.Parse(ticketIdStr);
            var ticket = await _ticketsRepository.GetTicketById(ticketId);

            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket does not exist"));

            if (ticket.AssignedToUserId != null)
                return Result.Fail(new BadRequestError("The ticket was already accepted by an Agent"));

            ticket.PriorityId = ticketsUpdateDto.PriorityId;

            _ticketsRepository.Update(ticket);
            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new OkSuccess("Ticket priority updated successfully."));
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
            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new OkSuccess("Ticket assigned successfully."));
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
            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new AcceptedSuccess("Ticket accepted successfully."));
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
            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.ClosedAt = DateTime.UtcNow;

            _ticketsRepository.Update(ticket);
            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new OkSuccess("Ticket closed successfully."));
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
            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.ClosedAt = null;

            _ticketsRepository.Update(ticket);
            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new OkSuccess("Ticket reopened successfully."));
        }
    }
}
