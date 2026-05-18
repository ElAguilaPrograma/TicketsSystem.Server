using TicketsSystem.Core.Services.AiProviders;

namespace TicketsSystem.Core.Interfaces;

public interface IMcpAiProvider
{
    Task<McpAiResult> GetCompletionAsync(McpAiRequest request);
}
