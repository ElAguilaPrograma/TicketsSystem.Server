namespace TicketsSystem.Domain.Entities;

public class TicketHistory
{
    public Guid HistoryId { get; set; }
    public Guid TicketId { get; set; }
    public Guid ChangedByUserId { get; set; }
    public string FieldName { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }

    // Navigation properties
    public virtual User ChangedByUser { get; set; } = null!;
    public virtual Ticket Ticket { get; set; } = null!;
}
