using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Api.Controllers;

[Authorize(Roles = "Admin, Agent")]
[Route("api/[controller]")]
[ApiController]
public class McpController : ApiBaseController
{
    private readonly IMcpService _mcpService;

    public McpController(IMcpService mcpService)
    {
        _mcpService = mcpService;
    }

    [HttpGet("get-summary-from-ticket/{ticketId}")]
    public async Task<IActionResult> GetSummaryFromTicket(string ticketId)
        => ProcessResult(await _mcpService.GetTicketSummaryAsync(ticketId));

    [HttpGet("get-ai-insights")]
    public async Task<IActionResult> GetAiInsights()
        => ProcessResult(await _mcpService.GetAiInsightsAsync());
}
