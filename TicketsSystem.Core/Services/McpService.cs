using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using TicketsSystem.Core.DTOs.McpDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Core.Services.AiProviders;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Enums;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services;

public class McpService : IMcpService
{
    private readonly ITicketsRepository _ticketsRepository;
    private readonly ITicketCommentsRepository _commentsRepository;
    private readonly ITicketAttachmentRepository _attachmentRepository;
    private readonly IGenericRepository<Mcprequest> _mcpRequestRepository;
    private readonly IGenericRepository<Mcpresponse> _mcpResponseRepository;
    private readonly IMcpAiProvider _aiProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<McpService> _logger;

    public McpService(
        ITicketsRepository ticketsRepository,
        ITicketCommentsRepository commentsRepository,
        ITicketAttachmentRepository attachmentRepository,
        IGenericRepository<Mcprequest> mcpRequestRepository,
        IGenericRepository<Mcpresponse> mcpResponseRepository,
        IMcpAiProvider aiProvider,
        IUnitOfWork unitOfWork,
        ILogger<McpService> logger)
    {
        _ticketsRepository = ticketsRepository;
        _commentsRepository = commentsRepository;
        _attachmentRepository = attachmentRepository;
        _mcpRequestRepository = mcpRequestRepository;
        _mcpResponseRepository = mcpResponseRepository;
        _aiProvider = aiProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TicketSummaryResponse>> GetTicketSummaryAsync(string ticketIdStr)
    {
        if (!Guid.TryParse(ticketIdStr, out var ticketId))
            return Result.Fail(new BadRequestError("Invalid ticket id format"));

        var ticket = await _ticketsRepository.GetTicketById(ticketId);
        if (ticket == null)
            return Result.Fail(new NotFoundError("Ticket not found"));

        var comments = await _commentsRepository.GetTicketComments(ticketId);
        var attachments = await _attachmentRepository.GetTicketAttachmentsByTicketId(ticketId);

        var keywords = ticket.Title
            .Split([' ', '.', ',', '!', '?', ':', ';', '-', '_'], StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var similarTickets = await _ticketsRepository.GetSimilarTicketsAsync(ticketId, keywords);

        var systemPrompt = """
            You are a technical support ticket analyst. Analyze the provided ticket and generate:
            1. A clear and concise summary of the problem (in English)
            2. Possible solutions or steps to follow

            Respond ONLY with a valid JSON object in the following format, without additional text:
            {
              "summary": "Summary of the problem here",
              "solutions": ["Solution 1", "Solution 2", "Solution 3"]
            }
            """;

        var commentsText = comments.Any()
            ? string.Join("\n", comments.Select(c => $"- [{c.CreatedAt:yyyy-MM-dd}] {c.Content}"))
            : "No comments available.";

        var userPrompt = $"""
            Title: {ticket.Title}
            Description: {ticket.Description}
            Status: {ticket.Status.Name}
            Priority: {ticket.Priority.Name}
            Created by: {ticket.CreatedByUser.FullName}
            Assigned to: {ticket.AssignedToUser?.FullName ?? "Not assigned"}
            Creation date: {ticket.CreatedAt:yyyy-MM-dd HH:mm}

            Ticket comments:
            {commentsText}

            Attachments: {attachments.Count()} file(s)

            Similar tickets found: {similarTickets.Count()}
            """;

        var aiRequest = new McpAiRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt
        };

        var mcpRequest = new Mcprequest
        {
            TicketId = ticketId,
            UseCase = "ticket_summary",
            PromptVersion = "1.0"
        };

        await _mcpRequestRepository.Create(mcpRequest);

        McpAiResult aiResult;
        try
        {
            aiResult = await _aiProvider.GetCompletionAsync(aiRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI provider call failed for ticket {TicketId}", ticketId);
            return Result.Fail(new InternalServerError("Failed to get AI response"));
        }

        var mcpResponse = new Mcpresponse
        {
            McprequestId = mcpRequest.McprequestId,
            ResponseType = "summary",
            Confidence = aiResult.Confidence,
            Payload = aiResult.Content
        };

        await _mcpResponseRepository.Create(mcpResponse);

        string summary;
        List<string> solutions;

        try
        {
            using var doc = JsonDocument.Parse(aiResult.Content);
            var root = doc.RootElement;
            summary = root.TryGetProperty("summary", out var s) ? s.GetString() ?? "" : "";
            solutions = root.TryGetProperty("solutions", out var sols)
                ? sols.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => !string.IsNullOrEmpty(x)).ToList()
                : [];
        }
        catch (JsonException)
        {
            _logger.LogWarning("Failed to parse AI response as JSON for ticket {TicketId}. Using raw content.", ticketId);
            summary = aiResult.Content;
            solutions = [];
        }

        if (string.IsNullOrWhiteSpace(summary))
            summary = aiResult.Content;

        await _unitOfWork.SaveChangesAsync();

        var result = new TicketSummaryResponse
        {
            TicketId = ticket.TicketId,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.Name,
            Priority = ticket.Priority.Name,
            CreatedBy = ticket.CreatedByUser.FullName,
            AssignedTo = ticket.AssignedToUser?.FullName,
            CreatedAt = ticket.CreatedAt,
            CommentsCount = comments.Count(),
            AttachmentsCount = attachments.Count(),
            AiSummary = summary,
            ProposedSolutions = solutions,
            SimilarTickets = similarTickets.Select(st => new SimilarTicketDto
            {
                TicketId = st.TicketId,
                Title = st.Title,
                Status = st.Status.Name,
                Priority = st.Priority.Name,
                CreatedAt = st.CreatedAt
            }).ToList()
        };

        return Result.Ok(result);
    }

    public async Task<Result<AiInsightsResponse>> GetAiInsightsAsync()
    {
        var agingTickets = await _ticketsRepository.GetAgingTicketsAsync(7);
        var allTicketsPage = await _ticketsRepository.GetAllTicketsPaginatedWithFilters(1, 1000);

        var agingTicketDtos = agingTickets.Select(t => new AgingTicketDto
        {
            TicketId = t.TicketId,
            Title = t.Title,
            Status = t.Status.Name,
            Priority = t.Priority.Name,
            CreatedAt = t.CreatedAt,
            DaysOpen = (int)(DateTime.UtcNow - t.CreatedAt).TotalDays,
            AssignedTo = t.AssignedToUser?.FullName
        }).ToList();

        var recentTickets = allTicketsPage.Tickets
            .Where(t => t.CreatedAt >= DateTime.UtcNow.AddDays(-30))
            .ToList();

        var titleGroups = recentTickets
            .GroupBy(t =>
            {
                var words = t.Title.Split([' ', '.', ',', '!', '?', ':', ';'], StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 3
                    ? string.Join(" ", words.Take(3)).ToLower()
                    : t.Title.ToLower();
            })
            .Where(g => g.Count() > 1)
            .Select(g => new RecurringPatternDto
            {
                Pattern = g.Key,
                TicketCount = g.Count(),
                TicketTitles = g.Select(t => t.Title).ToList()
            })
            .ToList();

        var systemPrompt = """
            Eres un analista de soporte técnico senior. Revisa los datos de tickets proporcionados y genera un análisis en español que incluya:
            1. Observaciones sobre tickets que llevan mucho tiempo abiertos
            2. Patrones detectados de tickets repetitivos
            3. Recomendaciones para mejorar la gestión

            Sé específico y menciona datos concretos. No uses JSON, responde en texto plano con formato markdown.
            """;

        var agingText = agingTicketDtos.Any()
            ? string.Join("\n", agingTicketDtos.Select(t => $"- #{t.TicketId}: \"{t.Title}\" - {t.DaysOpen} días abierto, {t.Status}, asignado a: {t.AssignedTo ?? "nadie"}"))
            : "No hay tickets con más de 7 días abiertos.";

        var patternsText = titleGroups.Any()
            ? string.Join("\n", titleGroups.Select(p => $"- Patrón: \"{p.Pattern}\" - {p.TicketCount} tickets repetidos"))
            : "No se detectaron patrones repetitivos claros.";

        var userPrompt = $"""
            Tickets con más de 7 días abiertos ({agingTicketDtos.Count} tickets):
            {agingText}

            Patrones de tickets repetitivos (últimos 30 días):
            {patternsText}

            Total de tickets: {allTicketsPage.TotalCount}
            """;

        var aiRequest = new McpAiRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt
        };

        var mcpRequest = new Mcprequest
        {
            TicketId = null,
            UseCase = "ai_insights",
            PromptVersion = "1.0"
        };

        await _mcpRequestRepository.Create(mcpRequest);

        McpAiResult aiResult;
        try
        {
            aiResult = await _aiProvider.GetCompletionAsync(aiRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI provider call failed for insights");
            return Result.Fail(new InternalServerError("Failed to get AI insights"));
        }

        var mcpResponse = new Mcpresponse
        {
            McprequestId = mcpRequest.McprequestId,
            ResponseType = "insights",
            Confidence = aiResult.Confidence,
            Payload = aiResult.Content
        };

        await _mcpResponseRepository.Create(mcpResponse);
        await _unitOfWork.SaveChangesAsync();

        var result = new AiInsightsResponse
        {
            AgingTickets = agingTicketDtos,
            RecurringPatterns = titleGroups,
            AiAnalysis = aiResult.Content
        };

        return Result.Ok(result);
    }
}
