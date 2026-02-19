namespace TicketsSystem.Core.DTOs.TicketsDTO;

public class TicketsUpdateDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int StatusId { get; set; }
    public int PriorityId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
