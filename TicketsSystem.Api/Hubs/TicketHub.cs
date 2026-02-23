using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace TicketsSystem.Api.Hubs
{
    public class TicketHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Admin" || role == "Agent")
                await Groups.AddToGroupAsync(Context.ConnectionId, "ManagementGroup");

            if (role == "User")
                await Groups.AddToGroupAsync(Context.ConnectionId, "UsersGroup");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
