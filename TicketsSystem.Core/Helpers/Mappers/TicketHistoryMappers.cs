using TicketsSystem.Core.DTOs.TicketsHistoryDTO;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Core.Helpers.Mappers;

public static class TicketHistoryMappers
{
    public static TicketHistoryReadDto ToReadDto(this TicketHistory history) => new()
    {
        TicketId = history.TicketId,
        ChangedByUserId = history.ChangedByUserId,
        ChangeGroupId = history.ChangeGroupId,
        FieldName = history.FieldName,
        OldValue = history.OldValue,
        NewValue = history.NewValue,
        ChangedAt = history.ChangedAt
    };

    public static TicketHistoryGroupDto ToGroupDto(this IGrouping<Guid, TicketHistory> group)
    {
        var first = group.First();

        return new TicketHistoryGroupDto
        {
            ChangeGroupId = group.Key,
            ChangedAt = first.ChangedAt,
            ChangedByUserId = first.ChangedByUserId,
            ChangedByUserFullName = first.ChangedByUser.FullName,
            Changes = group.Select(t => t.ToReadDto())
        };
    }
}
