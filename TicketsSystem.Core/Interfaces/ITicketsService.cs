using FluentResults;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.DTOs.TicketsDTO;

namespace TicketsSystem.Core.Interfaces
{
    public interface ITicketsService
    {
        Task<Result> AcceptTickets(string ticketIdStr);
        Task<Result> AssingTicketAsync(string userIdStr, string ticketIdSrt);
        Task<Result> CreateATicketAsync(TicketsCreateDto ticketsCreateDto);
        Task<Result<IEnumerable<TicketsReadDto>>> GetAllTicketsAsync();
        Task<Result<IEnumerable<TicketsReadDto>>> GetCurrentUserTicketsAsync();
        Task<Result<IEnumerable<TicketsReadDto>>> GetTicketsByUserIdAsync(string userIdStr);
        Task<Result<IEnumerable<TicketsReadDto>>> SearchTicketsAsync(string query, int? statusId, int? priorityId);
        Task<Result> UpdateATicketInfoAsync(TicketsUpdateDto ticketsUpdateDto, string ticketIdStr);
        Task<Result> UpdateTicketPriority([FromBody] TicketsUpdateDto ticketsUpdateDto, string ticketIdStr);
    }
}
