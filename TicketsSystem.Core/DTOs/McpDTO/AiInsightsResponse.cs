namespace TicketsSystem.Core.DTOs.McpDTO;

public class AiInsightsResponse
{
    public List<AgingTicketDto> AgingTickets { get; set; } = [];
    public List<RecurringPatternDto> RecurringPatterns { get; set; } = [];
    public string AiAnalysis { get; set; } = null!;
}
