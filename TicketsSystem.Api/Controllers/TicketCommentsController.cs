using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.DTOs.TicketsCommentsDTO;
using TicketsSystem.Core.Services;

namespace TicketsSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketCommentsController : ApiBaseController
    {
        private readonly ITicketCommetsService _ticketCommetsService;
        public TicketCommentsController(ITicketCommetsService ticketCommetsService)
        {
            _ticketCommetsService = ticketCommetsService;
        }

        [HttpPost("createticketcomment/{ticketId}")]
        public async Task<IActionResult> CreateTicketComment(string ticketId, [FromBody] TicketsCreateComment ticketsCreateComment)
            => ProcessResult(await _ticketCommetsService.CreateTicketCommentAsync(ticketId, ticketsCreateComment));

        [HttpGet("getticketscomment/{ticketId}")]
        public async Task<IActionResult> GetTicketComment(string ticketId)
            => ProcessResult(await _ticketCommetsService.GetTicketCommentsAsync(ticketId));

        [HttpPost("updateticketcommnet/{commentId}")]
        public async Task<IActionResult> UpdateTicketComment([FromBody] TickersUpdateComment ticketsUpdateComment, string commentId)
            => ProcessResult(await _ticketCommetsService.UpdateTicketCommentAsync(ticketsUpdateComment, commentId));
    }
}
