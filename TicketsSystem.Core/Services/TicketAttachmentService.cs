using FluentResults;
using TicketsSystem.Core.DTOs.TicketsAttachmentDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Core.Mappers;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{
    public class TicketAttachmentService : ITicketAttachmentService
    {
        private readonly ITicketAttachmentRepository _ticketsAttachmentRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IUnitOfWork _unitOfWork;
        public TicketAttachmentService(
            ITicketAttachmentRepository ticketAttachmentRepository, 
            ITicketsRepository ticketsRepository, 
            IUnitOfWork unitOfWork)
        {
            _ticketsAttachmentRepository = ticketAttachmentRepository;
            _ticketsRepository = ticketsRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IEnumerable<TicketAttachmentReadDTO>>> GetTicketAttachmentsByTicketIdAsync(string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("Ticket ID cannot be null or empty."));

            if (!Guid.TryParse(ticketIdStr, out Guid ticketId))
                return Result.Fail(new BadRequestError("Ticket ID is invalid."));

            var ticket = await _ticketsRepository.GetById(ticketId);

            if (ticket == null)
                return Result.Fail(new NotFoundError("Ticket not found."));

            var attachments = await _ticketsAttachmentRepository.GetTicketAttachmentsByTicketId(ticketId);

            var attachmentDtos = attachments.Select(attachment => attachment.ToReadDto());

            return Result.Ok(attachmentDtos);
        }

        public async Task<Result> AddTicketAttachmentAsync(string ticketIdStr, TicketsAttachmentCreateDto attachmentCreateDto, bool saveChanges = true)
        {
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("Ticket ID cannot be null or empty."));

            if (!Guid.TryParse(ticketIdStr, out Guid ticketId))
                return Result.Fail(new BadRequestError("Ticket ID is invalid."));

            if (saveChanges) 
            {
                var ticket = await _ticketsRepository.GetById(ticketId);

                if (ticket == null)
                    return Result.Fail(new NotFoundError("Ticket not found."));
            }

            var newAttachment = new TicketAttachment
            {
                TicketId = ticketId,
                FileName = attachmentCreateDto.FileName,
                FileUrl = attachmentCreateDto.FileUrl,
            };

            await _ticketsAttachmentRepository.Create(newAttachment);
            
            if (saveChanges)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return Result.Ok();
        }

        public async Task<Result> DeleteTicketAttachmentAsync(string attachmentIdStr, string ticketIdStr)
        {
            if (string.IsNullOrWhiteSpace(attachmentIdStr))
                return Result.Fail(new BadRequestError("Attachment ID cannot be null or empty."));
                
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("Ticket ID cannot be null or empty."));

            if (!Guid.TryParse(attachmentIdStr, out Guid attachmentId))
                return Result.Fail(new BadRequestError("Attachment ID is invalid."));

            if (!Guid.TryParse(ticketIdStr, out Guid ticketId))
                return Result.Fail(new BadRequestError("Ticket ID is invalid."));

            var attachment = await _ticketsAttachmentRepository.GetById(attachmentId);
            var ticket = await _ticketsRepository.GetById(ticketId);

             if (ticket == null)
                return Result.Fail(new NotFoundError("Ticket not found."));

            if (attachment == null)
                return Result.Fail(new NotFoundError("Attachment not found."));

            await _ticketsAttachmentRepository.DeleteAttachmentFromTicket(ticket.TicketId, attachment.TicketAttachmentId);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok();
        }
    }
}