
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.DTOs.TicketsAttachmentDTO;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketAttachmentsController : ApiBaseController
    {
        private readonly ITicketAttachmentService _ticketAttachmentService;
        public TicketAttachmentsController(ITicketAttachmentService ticketAttachmentService)
        {
            _ticketAttachmentService = ticketAttachmentService;
        }

        [HttpGet("getattachments/{ticketId}")]
        [Authorize]
        public async Task<IActionResult> GetTicketAttachments(string ticketId)
            => ProcessResult(await _ticketAttachmentService.GetTicketAttachmentsByTicketIdAsync(ticketId));

        [HttpPost("addattachment/{ticketId}")]
        [Authorize]
        public async Task<IActionResult> AddTicketAttachment(string ticketId, TicketsAttachmentCreateDto ticketsAttachmentCreateDto)
            => ProcessResult(await _ticketAttachmentService.AddTicketAttachmentAsync(ticketId, ticketsAttachmentCreateDto));

        [HttpDelete("deleteattachment/{ticketId}/{attachmentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTicketAttachment(string ticketId, string attachmentId)
            => ProcessResult(await _ticketAttachmentService.DeleteTicketAttachmentAsync(attachmentId, ticketId));
    }
}