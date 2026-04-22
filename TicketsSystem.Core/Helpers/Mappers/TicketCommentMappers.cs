using TicketsSystem.Core.DTOs.TicketsCommentsDTO;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Core.Helpers.Mappers;

public static class TicketCommentMappers
{
    public static TicketComment ToEntity(this TicketsCreateComment dto, Guid ticketId, Guid userId) => new()
    {
        TicketId = ticketId,
        UserId = userId,
        Content = dto.Content,
        IsInternal = dto.IsInternal,
        CreatedAt = DateTime.UtcNow
    };

    public static TicketsReadComment ToReadDto(this TicketComment comment, string createdByUser) => new()
    {
        CommentId = comment.CommentId,
        TicketId = comment.TicketId,
        UserId = comment.UserId,
        Content = comment.Content,
        IsInternal = comment.IsInternal,
        CreatedByUser = createdByUser,
        CreatedAt = comment.CreatedAt
    };

    public static TicketsReadComment ToReadDto(this TicketComment comment) => new()
    {
        CommentId = comment.CommentId,
        TicketId = comment.TicketId,
        UserId = comment.UserId,
        Content = comment.Content,
        IsInternal = comment.IsInternal,
        CreatedByUser = comment.User.FullName,
        CreatedAt = comment.CreatedAt
    };
}
