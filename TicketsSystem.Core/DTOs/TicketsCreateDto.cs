namespace TicketsSystem.Core.DTOs;

public class TicketsCreateDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int StatusId { get; set; }
    public int PriorityId { get; set; }
    public Guid CreatedByUserId { get; set; }
}
