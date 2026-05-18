using FluentResults;
using TicketsSystem.Core.DTOs.McpDTO;

namespace TicketsSystem.Core.Interfaces;

public interface IMcpService
{
    Task<Result<TicketSummaryResponse>> GetTicketSummaryAsync(string ticketIdStr);
    Task<Result<AiInsightsResponse>> GetAiInsightsAsync();
}
