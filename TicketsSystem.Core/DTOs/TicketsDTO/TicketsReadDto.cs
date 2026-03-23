namespace TicketsSystem.Core.DTOs.TicketsDTO;

public class TicketsReadDto
{
    public Guid TicketId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int StatusId { get; set; }
    public string? StatusName { get; set; } = "To be defined";
    public int PriorityId { get; set; }
    public string? PriorityName { get; set; } = "To be defined";
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUser { get; set; } = "To be defined";
    public string CreatedByUser { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
