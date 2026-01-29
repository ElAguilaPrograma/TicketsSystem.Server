namespace TicketsSystem.Core.DTOs;

public class TicketsReadDto
{
    public Guid TicketId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? StatusName { get; set; } = "To be defined";
    public string? PriorityName { get; set; } = "To be defined";
    public string? AssignedToUser { get; set; } = "To be defined";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
