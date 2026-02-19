using FluentResults;
using TicketsSystem.Core.DTOs.TicketsCommentsDTO;

namespace TicketsSystem.Core.Interfaces
{
    public interface ITicketCommetsService
    {
        Task<Result<TicketsCreateComment>> CreateTicketCommentAsync(string ticketIdStr, TicketsCreateComment ticketsCreateComment);
        Task<Result<IEnumerable<TicketsReadComment>>> GetTicketCommentsAsync(string ticketIdStr);
        Task<Result> UpdateTicketCommentAsync(TickersUpdateComment ticketsUpdateComment, string ticketCommentIdStr);
    }
}
