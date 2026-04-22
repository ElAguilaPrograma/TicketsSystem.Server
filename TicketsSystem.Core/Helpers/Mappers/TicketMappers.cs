using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Enums;

namespace TicketsSystem.Core.Helpers.Mappers;

public static class TicketMappers
{
    public static Ticket ToEntity(this TicketsCreateDto dto, Guid createdByUserId) => new()
    {
        Title = dto.Title,
        Description = dto.Description,
        StatusId = (int)TicketsStatusValue.Open,
        PriorityId = dto.PriorityId,
        CreatedByUserId = createdByUserId,
        CreatedAt = DateTime.UtcNow
    };

    public static TicketsReadDto ToReadDto(this Ticket ticket) => new()
    {
        TicketId = ticket.TicketId,
        Title = ticket.Title,
        Description = ticket.Description,
        StatusId = ticket.StatusId,
        StatusName = ticket.Status.Name,
        PriorityId = ticket.PriorityId,
        PriorityName = ticket.Priority.Name,
        AssignedToUserId = ticket.AssignedToUserId,
        AssignedToUser = ticket.AssignedToUser?.FullName,
        CreatedByUser = ticket.CreatedByUser.FullName,
        CreatedByUserId = ticket.CreatedByUserId,
        CreatedAt = ticket.CreatedAt,
        UpdatedAt = ticket.UpdatedAt,
        ClosedAt = ticket.ClosedAt
    };
}
