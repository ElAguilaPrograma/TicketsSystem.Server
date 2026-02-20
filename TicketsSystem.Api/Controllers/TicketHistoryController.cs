using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketHistoryController : ApiBaseController
    {
        private readonly ITicketHistoryService _ticketHistoryService;
        public TicketHistoryController(ITicketHistoryService ticketHistoryService)
        {
            _ticketHistoryService = ticketHistoryService;
        }

        [HttpGet("gettickethistory/{ticketId}")]
        public async Task<IActionResult> GetTicketHistory(string ticketId)
            => ProcessResult(await _ticketHistoryService.GetTicketHistoryAsync(ticketId));
    }
}
