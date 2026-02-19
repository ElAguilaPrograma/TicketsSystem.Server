using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Services;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ApiBaseController
    {
        private readonly ITicketsService _ticketsService;
        public TicketsController(ITicketsService ticketsService)
        {
            _ticketsService = ticketsService;
        }

        [HttpGet("getalltickets")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTickets()
            => ProcessResult(await _ticketsService.GetAllTicketsAsync());

        [HttpGet("getcurrentusertickets")]
        public async Task<IActionResult> GetCurrentUserTickets()
            => ProcessResult(await _ticketsService.GetCurrentUserTicketsAsync());

        [HttpGet("getticketsbyuserid/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTicketsByUserId(string userId)
            => ProcessResult(await _ticketsService.GetTicketsByUserIdAsync(userId));

        [HttpPost("createticket")]
        public async Task<IActionResult> CreateTicket([FromBody] TicketsCreateDto ticketsCreateDto)
            => ProcessResult(await _ticketsService.CreateATicketAsync(ticketsCreateDto));

        [HttpPost("updateticketinfo/{ticketId}")]
        [Authorize(Roles = "Admin, Agent")]
        public async Task<IActionResult> UpdateTicket([FromBody] TicketsUpdateDto tickets, string ticketId)
            => ProcessResult(await _ticketsService.UpdateATicketInfoAsync(tickets, ticketId));

        [HttpPost("updateticketpriority")]
        public async Task<IActionResult> UpdateTicketPriorityLevel([FromBody] TicketsUpdateDto ticketsUpdateDto, string ticketId)
            => ProcessResult(await _ticketsService.UpdateTicketPriority(ticketsUpdateDto, ticketId));

        [HttpPost("assingtickets/{ticketId}/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssingTickets(string userId, string ticketId)
            => ProcessResult(await _ticketsService.AssingTicketAsync(userId, ticketId));

        [HttpPost("accepttickets/{ticketId}")]
        [Authorize(Roles = "Admin, Agent")]
        public async Task<IActionResult> AcceptTicket(string ticketId)
            => ProcessResult(await _ticketsService.AcceptTickets(ticketId));

        [HttpPost("searchtickets/{query}")]
        public async Task<IActionResult> SearchTickets(string query, int? statusId, int? priorityId)
            => ProcessResult(await _ticketsService.SearchTicketsAsync(query, statusId, priorityId));
    }
}
