namespace TicketsSystem.Domain.Entities;

public class Mcprequest
{
    public Guid McprequestId { get; set; }
    public Guid TicketId { get; set; }
    public string UseCase { get; set; } = null!;
    public string PromptVersion { get; set; } = null!;
    public DateTime RequestedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Mcpresponse> Mcpresponses { get; set; } = [];
    public virtual Ticket Ticket { get; set; } = null!;
}
