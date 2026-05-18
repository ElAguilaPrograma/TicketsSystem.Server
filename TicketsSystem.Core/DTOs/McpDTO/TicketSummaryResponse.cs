namespace TicketsSystem.Core.DTOs.McpDTO;

public class TicketSummaryResponse
{
    public Guid TicketId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CommentsCount { get; set; }
    public int AttachmentsCount { get; set; }
    public string AiSummary { get; set; } = null!;
    public List<string> ProposedSolutions { get; set; } = [];
    public List<SimilarTicketDto> SimilarTickets { get; set; } = [];
}
