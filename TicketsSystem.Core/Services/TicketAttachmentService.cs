using FluentResults;
using Microsoft.AspNetCore.Http;
using TicketsSystem.Core.DTOs.TicketsAttachmentDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Core.Mappers;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Enums;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{
    public class TicketAttachmentService : ITicketAttachmentService
    {
        private readonly ITicketAttachmentRepository _ticketsAttachmentRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;
        public TicketAttachmentService(
            ITicketAttachmentRepository ticketAttachmentRepository, 
            ITicketsRepository ticketsRepository, 
            IStorageService fileStorageService,
            IUnitOfWork unitOfWork)
        {
            _ticketsAttachmentRepository = ticketAttachmentRepository;
            _ticketsRepository = ticketsRepository;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
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

            if (!attachmentDtos.Any())
                return Result.Ok(attachmentDtos).WithSuccess("No attachments found for this ticket.");

            return Result.Ok(attachmentDtos);
        }

        public async Task<Result> AddTicketAttachmentAsync(string ticketIdStr, IFormFile file, bool saveChanges = true)
        {

            // HACER QUE LA URL SE GENERE CADA QUE EL CLIENTE LA PIDA PASANDO SOLO EL PATH Y EL BUCKET ASI NO ME LIO CON FECHAS DE EXPIRACIÓN.

            // Cambiar el dto por un file y si change es true manejar la subida del archivo desde aqui
            if (string.IsNullOrWhiteSpace(ticketIdStr))
                return Result.Fail(new BadRequestError("Ticket ID cannot be null or empty."));
            
            if (!Guid.TryParse(ticketIdStr, out Guid ticketId))
                return Result.Fail(new BadRequestError("Ticket ID is invalid."));

            var ticket = await _ticketsRepository.GetById(ticketId);

            if (ticket == null)
                return Result.Fail(new NotFoundError("Ticket not found."));

            var uploadFile = await _fileStorageService.UploadAsync(nameof(StorageBucket.TicketAttachments), file);

            if (uploadFile.IsFailed)
                return Result.Fail(new InternalServerError("An error occurred while uploading the file."));

            var (path, fileName) = uploadFile.Value;

            var attachment = new TicketAttachment
            {
                TicketId = ticketId,
                FileName = fileName,
                FileUrl = string.Empty, // La URL se generará dinámicamente al solicitarla, usando el path y el bucket.
                Path = path
            };

            await _ticketsAttachmentRepository.Create(attachment);

            if (saveChanges)
                await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new OkSuccess("Attachment added successfully."));
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
            await _fileStorageService.DeleteFileAsync(attachment.Path, nameof(StorageBucket.TicketAttachments));
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok();
        }
    }
}