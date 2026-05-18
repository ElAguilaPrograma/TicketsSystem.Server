namespace TicketsSystem.Core.Services.AiProviders;

public class McpAiResult
{
    public string Content { get; set; } = null!;
    public decimal? Confidence { get; set; }
    public string? ModelUsed { get; set; }
}
