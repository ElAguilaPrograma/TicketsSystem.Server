namespace TicketsSystem.Domain.Entities;

public class Mcpresponse
{
    public Guid McpresponseId { get; set; }
    public Guid McprequestId { get; set; }
    public string? ResponseType { get; set; }
    public decimal? Confidence { get; set; }
    public string? Payload { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Mcprequest Mcprequest { get; set; } = null!;
}
