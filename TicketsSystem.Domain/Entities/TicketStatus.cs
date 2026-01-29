namespace TicketsSystem.Domain.Entities;

public class TicketStatus
{
    public int StatusId { get; set; }
    public string Name { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<Ticket> Tickets { get; set; } = [];
}
