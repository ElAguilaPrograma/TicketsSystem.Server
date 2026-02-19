namespace TicketsSystem.Core.DTOs.TicketsDTO;

public class TicketsCreateDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int PriorityId { get; set; }
    public Guid CreatedByUserId { get; set; }
}
