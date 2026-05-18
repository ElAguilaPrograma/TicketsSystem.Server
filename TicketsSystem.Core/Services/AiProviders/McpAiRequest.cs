namespace TicketsSystem.Core.Services.AiProviders;

public class McpAiRequest
{
    public string SystemPrompt { get; set; } = null!;
    public string UserPrompt { get; set; } = null!;
    public string? Model { get; set; }
}
