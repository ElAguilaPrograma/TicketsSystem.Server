using ClosedXML.Excel;
using FluentResults;
using TicketsSystem.Core.DTOs.NotificationDTO;
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
        private readonly INotificationService _notificationService;

        public TicketsService(ITicketsRepository ticketsRepository,
            ICurrentUserService currentUserService,
            IGetUserRole getUserRole,
            IUserRepository userRepository,
            ITicketsHistoryRepository ticketsHistoryRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _ticketsRepository = ticketsRepository;
            _currentUserService = currentUserService;
            _getUseRole = getUserRole;
            _userRepository = userRepository;
            _ticketsHistoryRepository = ticketsHistoryRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<Result<PagedResult<TicketsReadDto>>> GetAllTicketsWithFiltersAsync(GetAllTicketsFilterDto filterDto)
        {
            Guid? filterByUserId = null;
            Guid? filterByAssignedToUserId = null;

            if (!filterDto.CurrentUserOnly && _currentUserService.GetCurrentUserRole() == "User")
                return Result.Fail(new ForbiddenError("You are not authorized to perform this action."));
            if (!filterDto.AssignedToMeOnly && _currentUserService.GetCurrentUserRole() == "User")
                return Result.Fail(new ForbiddenError("You are not authorized to perform this action."));
            if (filterDto.CurrentUserOnly)
                filterByUserId = _currentUserService.GetCurrentUserId();
            else if (!string.IsNullOrEmpty(filterDto.UserId))
                filterByUserId = Guid.Parse(filterDto.UserId);

            if (filterDto.AssignedToMeOnly)
                filterByAssignedToUserId = _currentUserService.GetCurrentUserId();

            var (tickets, totalCount) = await _ticketsRepository.GetAllTicketsPaginatedWithFilters(
                filterDto.Page,
                filterDto.PageSize,
                filterDto.Status,
                filterDto.Priority,
                filterDto.QuerySearch,
                filterDto.Month,
                filterDto.Year,
                filterByUserId,
                filterDto.HasAssignment,
                filterByAssignedToUserId);

            var result = new PagedResult<TicketsReadDto>
            {
                Data = tickets.Select(MapToDto),
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

            IEnumerable<TicketsReadDto> ticketsDTOs = tickets.Select(MapToDto);

            return Result.Ok(ticketsDTOs).WithSuccess(new OkSuccess("User tickets retrieved successfully."));
        }

        public async Task<Result<IEnumerable<TicketsReadDto>>> GetTicketsByUserIdAsync(string userIdStr)
        {
            Guid userId = Guid.Parse(userIdStr);

            var tickets = await _ticketsRepository.GetTicketsByUserId(userId);

            IEnumerable<TicketsReadDto> ticketsDTOs = tickets.Select(MapToDto);

            return Result.Ok(ticketsDTOs).WithSuccess(new OkSuccess("User tickets retrieved successfully."));
        }

        public async Task<Result<PagedResult<TicketsReadDto>>> GetMyAssignedTicketsAsync(GetAllTicketsFilterDto filterDto)
        {
            Guid currentUserId = _currentUserService.GetCurrentUserId();

            var (tickets, totalCount) = await _ticketsRepository.GetAllTicketsPaginatedWithFilters(
                filterDto.Page,
                filterDto.PageSize,
                filterDto.Status,
                filterDto.Priority,
                filterDto.QuerySearch,
                filterDto.Month,
                filterDto.Year,
                assignedToUserId: currentUserId,
                hasAssignment: true);

            var result = new PagedResult<TicketsReadDto>
            {
                Data = tickets.Select(MapToDto),
                TotalCount = totalCount,
                Page = filterDto.Page,
                PageSize = filterDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / filterDto.PageSize)
            };

            return Result.Ok(result);
        }

        public async Task<Result> CreateATicketAsync(TicketsCreateDto ticketsCreateDto)
        {

            Guid userId = _currentUserService.GetCurrentUserId();

            var newTicket = new Ticket
            {
                Title = ticketsCreateDto.Title,
                Description = ticketsCreateDto.Description,
                StatusId = (int)TicketsStatusValue.Open,
                PriorityId = ticketsCreateDto.PriorityId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
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

            // Notify via SignalR and save to DB
            var ticketWithData = await _ticketsRepository.GetTicketById(newTicket.TicketId);
            if (ticketWithData != null)
            {
                var ticketReadDto = MapToDto(ticketWithData);
                var notificationDto = new NotificationCreateDto
                {
                    UserId = ticketWithData.CreatedByUserId,
                    Type = nameof(NotificationsTypes.NewTicket),
                    Message = $"A new ticket was creted: '{ticketWithData.Title}'",
                    IsRead = false,
                    Ticket = ticketReadDto
                };
                await _notificationService.CreateANotificationAsync(notificationDto);
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

            if (_currentUserService.GetCurrentUserId() != ticket.AssignedToUserId && _currentUserService.GetCurrentUserRole() == "Agent")
                return Result.Fail(new BadRequestError("Only the assigned agent can modify this ticket."));

            int originalStatusId = ticket.StatusId;

            ticket.Title = ticketsUpdateDto.Title;
            ticket.Description = ticketsUpdateDto.Description;
            ticket.StatusId = ticketsUpdateDto.StatusId;
            ticket.PriorityId = ticketsUpdateDto.PriorityId;
            ticket.UpdatedAt = DateTime.UtcNow;

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

            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            if (ticketsUpdateDto.StatusId != originalStatusId)
            {
                var updatedTicket = await _ticketsRepository.GetTicketById(ticketId);
                if (updatedTicket != null)
                {
                    var newTicketReadDto = MapToDto(updatedTicket);

                    var notificationDto = new NotificationCreateDto
                    {
                        UserId = updatedTicket.CreatedByUserId,
                        Type = nameof(NotificationsTypes.UpdateTicket),
                        Message = $"The status of the ticket '{updatedTicket.Title}' has changed.",
                        IsRead = false,
                        Ticket = newTicketReadDto
                    };
                    await _notificationService.CreateANotificationAsync(notificationDto);
                }
            }

            return Result.Ok().WithSuccess(new OkSuccess("Ticket updated successfully."));
        }

        public async Task<Result> UpdateTicketUser(TicketsUpdateDto ticketsUpdateDto, string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("Ticket id is required"));

            Guid ticketId = Guid.Parse(ticketIdStr);
            var ticket = await _ticketsRepository.GetTicketById(ticketId);

            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket does not exist"));

            if (ticket.CreatedByUserId != _currentUserService.GetCurrentUserId())
                return Result.Fail(new BadRequestError("Only the user who created this ticket can modify it."));

            if (ticket.AssignedToUserId != null)
                return Result.Fail(new BadRequestError("The ticket was already accepted by an Agent"));

            if (ticketsUpdateDto.StatusId != ticket.StatusId || ticketsUpdateDto.AssignedToUserId != ticket.AssignedToUserId)
                return Result.Fail(new ForbiddenError("You are not allowed to perform this action. Only Agents or Administrator can update Status or Assigned To"));
                
            ticket.Title = ticketsUpdateDto.Title;
            ticket.Description = ticketsUpdateDto.Description;
            ticket.PriorityId = ticketsUpdateDto.PriorityId;
            ticket.UpdatedAt = DateTime.UtcNow;

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
            return await AssingTicketAsync(_currentUserService.GetCurrentUserId().ToString(), ticketIdStr);
        }

        public async Task<Result> CloseTicketsAsync(string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("The ticket id is not valid"));

            Guid ticketId = Guid.Parse(ticketIdStr);

            var ticket = await _ticketsRepository.GetById(ticketId);
            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket is does not exist"));

            if (ticket.StatusId == (int)TicketsStatusValue.Closed)
                return Result.Fail(new BadRequestError("The ticket is already close"));

            ticket.StatusId = (int)TicketsStatusValue.Closed;
            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.ClosedAt = DateTime.UtcNow;

            _ticketsRepository.Update(ticket);
            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            var closedTicket = await _ticketsRepository.GetTicketById(ticketId);
            if (closedTicket != null)
            {
                var ticketReadDto = MapToDto(closedTicket);
                var notificationDto = new NotificationCreateDto
                {
                    UserId = closedTicket.CreatedByUserId,
                    Type = nameof(NotificationsTypes.UpdateTicket),
                    Message = $"The ticket '{closedTicket.Title}' has been closed.",
                    IsRead = false,
                    Ticket = ticketReadDto
                };
                await _notificationService.CreateANotificationAsync(notificationDto);
            }

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

            if (ticket.StatusId != (int)TicketsStatusValue.Closed)
                return Result.Fail(new BadRequestError("The ticket is already open, please refresh the page."));

            ticket.StatusId = (int)TicketsStatusValue.Reopened;
            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.ClosedAt = null;

            _ticketsRepository.Update(ticket);
            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            var reopenedTicket = await _ticketsRepository.GetTicketById(ticketId);
            if (reopenedTicket != null)
            {
                var ticketReadDto = MapToDto(reopenedTicket);
                var notificationDto = new NotificationCreateDto
                {
                    UserId = reopenedTicket.CreatedByUserId,
                    Type = nameof(NotificationsTypes.UpdateTicket),
                    Message = $"The ticket '{reopenedTicket.Title}' has been reopened.",
                    IsRead = false,
                    Ticket = ticketReadDto
                };
                await _notificationService.CreateANotificationAsync(notificationDto);
            }

            return Result.Ok().WithSuccess(new OkSuccess("Ticket reopened successfully."));
        }

        public async Task<Result> AbandonATicketAsync(string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("The ticket id is not valid"));

            Guid ticketId = Guid.Parse(ticketIdStr);

            var ticket = await _ticketsRepository.GetTicketById(ticketId);
            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket is does not exist"));

            ticket.AssignedToUserId = null;
            _ticketsRepository.Update(ticket);
            await _ticketsHistoryRepository.TrackChanges(ticket, _currentUserService.GetCurrentUserId());
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new OkSuccess("Ticket was abandoned"));
        }

        public async Task<Result<GetCurrentUserTicketsCount>> GetCurrentUserTicketsCountAsync()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var role = _currentUserService.GetCurrentUserRole() ?? "";

            var counts = await _ticketsRepository.GetTicketsCountSummary(userId, role);

            return new GetCurrentUserTicketsCount
            {
                TotalTickets = counts.Values.Sum(),
                TicketsOpen = counts.GetValueOrDefault((int)TicketsStatusValue.Open),
                TicketsClosed = counts.GetValueOrDefault((int)TicketsStatusValue.Closed),
                TicketsReopen = counts.GetValueOrDefault((int)TicketsStatusValue.Reopened)
            };
        }

        public async Task<Result<int>> GetTodaysTicketsCountAsync()
        {
            var count = await _ticketsRepository.GetTodaysTicketsCount();
            return Result.Ok(count).WithSuccess("Today's tickets count retrieved successfully.");
        }

        public async Task<Result<TicketsReadDto>> GetTicketByIdAsync(string ticketIdStr)
        {
            Guid ticketId = Guid.Parse(ticketIdStr);

            var ticket = await _ticketsRepository.GetTicketById(ticketId);

            if (ticket == null)
                return Result.Fail(new NotFoundError("The ticket does not exist"));

            var ticketDto = MapToDto(ticket);

            return Result.Ok(ticketDto).WithSuccess("Ticket loaded correctly");
        }

        public async Task<Result<byte[]>> ExportTicketsAsync(FilterTicketsDto filterDto)
        {
            Guid? filterByUserId = null;
            Guid? filterByAssignedToUserId = null;

            if (!filterDto.CurrentUserOnly && _currentUserService.GetCurrentUserRole() == "User")
                return Result.Fail(new ForbiddenError("You are not authorized to perform this action."));
            if (!filterDto.AssignedToMeOnly && _currentUserService.GetCurrentUserRole() == "User")
                return Result.Fail(new ForbiddenError("You are not authorized to perform this action."));
            if (filterDto.CurrentUserOnly)
                filterByUserId = _currentUserService.GetCurrentUserId();
            else if (!string.IsNullOrWhiteSpace(filterDto.UserId))
                filterByUserId = Guid.Parse(filterDto.UserId);

            if (filterDto.AssignedToMeOnly)
                filterByAssignedToUserId = _currentUserService.GetCurrentUserId();

            var filterTickets = await _ticketsRepository.ExportTicketsWithFilters(
                filterDto.Status,
                filterDto.Priority,
                filterDto.QuerySearch,
                filterDto.Month,
                filterDto.Year,
                filterByUserId,
                filterDto.HasAssignment,
                filterByAssignedToUserId);

            var tickets = filterTickets.Select(t => new
            {
                TicketId = t.TicketId.ToString(),
                Title = t.Title,
                Status = t.Status.Name,
                Priority = t.Priority.Name,
                CreatedBy = t.CreatedByUser.FullName,
                AssignedTo = t.AssignedToUser?.FullName ?? "Unassigned",
                CreatedAt = t.CreatedAt.AddMinutes(-filterDto.TimezoneOffsetMinutes)
            });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Tickets");

                worksheet.Cell(1, 1).Value = "Ticket Id";
                worksheet.Cell(1, 2).Value = "Title";
                worksheet.Cell(1, 3).Value = "Status";
                worksheet.Cell(1, 4).Value = "Priority";
                worksheet.Cell(1, 5).Value = "Created By";
                worksheet.Cell(1, 6).Value = "Assigned To";
                worksheet.Cell(1, 7).Value = "Created At";

                worksheet.Cell(2, 1).InsertData(tickets);

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                return Result.Ok(stream.ToArray()).WithSuccess(new OkSuccess("Tickets exported successfully."));
            }
        }

        private static TicketsReadDto MapToDto(Ticket t) => new()
        {
            TicketId = t.TicketId,
            Title = t.Title,
            Description = t.Description,
            StatusId = t.StatusId,
            StatusName = t.Status.Name,
            PriorityId = t.PriorityId,
            PriorityName = t.Priority.Name,
            AssignedToUserId = t.AssignedToUserId,
            AssignedToUser = t.AssignedToUser?.FullName,
            CreatedByUser = t.CreatedByUser.FullName,
            CreatedByUserId = t.CreatedByUserId,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            ClosedAt = t.ClosedAt
        };
    }
}
