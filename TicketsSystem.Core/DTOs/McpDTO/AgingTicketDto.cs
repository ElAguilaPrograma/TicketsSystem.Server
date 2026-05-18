namespace TicketsSystem.Core.DTOs.McpDTO;

public class AgingTicketDto
{
    public Guid TicketId { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int DaysOpen { get; set; }
    public string? AssignedTo { get; set; }
}
