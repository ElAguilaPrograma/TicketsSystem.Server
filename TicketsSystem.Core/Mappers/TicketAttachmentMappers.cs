using TicketsSystem.Core.DTOs.TicketsAttachmentDTO;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Core.Mappers;

public static class TicketAttachmentMappers
{
    public static TicketAttachmentReadDTO ToReadDto(this TicketAttachment attachment) => new()
    {
        TicketAttachmentId = attachment.TicketAttachmentId,
        FileName = attachment.FileName,
        FileUrl = attachment.FileUrl,
        CreatedAt = attachment.CreatedAt
    };
}