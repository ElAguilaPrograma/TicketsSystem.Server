using System.Threading.Tasks;
using TicketsSystem.Core.DTOs.TicketsCommentsDTO;
using TicketsSystem.Core.DTOs.TicketsDTO;

namespace TicketsSystem.Core.Interfaces
{
    public interface ITicketHubService
    {
        Task NotifyTicketCommentCreated(TicketsReadComment comment, Guid ticketCreatorUserId);
        Task NotifyTicketCreated(TicketsReadDto ticket);
        Task NotifyTicketStatusChanged(TicketsReadDto ticket, Guid userId);
    }
}
