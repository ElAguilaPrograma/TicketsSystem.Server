using FluentResults;
using TicketsSystem.Core.DTOs.TicketsAttachmentDTO;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Core.Interfaces
{
    public interface ITicketAttachmentService
    {
        Task<Result<IEnumerable<TicketAttachmentReadDTO>>> GetTicketAttachmentsByTicketIdAsync(string ticketIdStr);
        Task<Result> AddTicketAttachmentAsync(string ticketIdStr, TicketsAttachmentCreateDto attachmentCreateDto, bool saveChanges = true);
        Task<Result> DeleteTicketAttachmentAsync(string attachmentIdStr, string ticketIdStr);
    }
}