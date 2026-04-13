using Microsoft.AspNetCore.SignalR;
using TicketsSystem.Api.Hubs;
using TicketsSystem.Core.DTOs.TicketsCommentsDTO;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Api.Services
{
    public class TicketHubService : ITicketHubService
    {
        private readonly IHubContext<TicketHub> _hubContext;

        public TicketHubService(IHubContext<TicketHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyTicketCreated(TicketsReadDto ticket)
            => await _hubContext.Clients.Group("AgentsGroup").SendAsync("ReceiveNewTicket", ticket);

        public async Task NotifyTicketStatusChanged(TicketsReadDto ticket, Guid userId)
            => await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNewTicketStatusChange", ticket);

        public async Task NotifyTicketCommentCreated(TicketsReadComment comment, Guid ticketCreatorUserId, Guid? assignedAgentUserId)
        {
            if (!comment.IsInternal && comment.UserId != ticketCreatorUserId)
                await _hubContext.Clients.User(ticketCreatorUserId.ToString()).SendAsync("ReceiveNewTicketComment", comment);

            if (assignedAgentUserId.HasValue && comment.UserId != assignedAgentUserId.Value)
                await _hubContext.Clients.User(assignedAgentUserId.Value.ToString()).SendAsync("ReceiveNewTicketComment", comment);
        }

    }
}
