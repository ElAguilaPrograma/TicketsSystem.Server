namespace TicketsSystem.Core.DTOs.McpDTO;

public class RecurringPatternDto
{
    public string Pattern { get; set; } = null!;
    public int TicketCount { get; set; }
    public List<string> TicketTitles { get; set; } = [];
}
