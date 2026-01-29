namespace TicketsSystem.Domain.Entities;

public class TicketPriority
{
    public int PriorityId { get; set; }
    public string Name { get; set; } = null!;
    public int Level { get; set; }

    // Navigation properties
    public virtual ICollection<Ticket> Tickets { get; set; } = [];
}
