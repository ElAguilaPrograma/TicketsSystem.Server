using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Core.Services.AiProviders;

public class OpenAiProvider : IMcpAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiProvider> _logger;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _model;

    public OpenAiProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAiProvider> logger,
        string apiKey,
        string endpoint,
        string model)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _apiKey = apiKey;
        _endpoint = endpoint;
        _model = model;
    }

    public async Task<McpAiResult> GetCompletionAsync(McpAiRequest request)
    {
        var model = request.Model ?? _model;

        var payload = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserPrompt }
            },
            temperature = 0.3
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        _logger.LogInformation("Sending request to AI provider. Model: {Model}, SystemPrompt length: {SystemLen}, UserPrompt length: {UserLen}",
            model, request.SystemPrompt.Length, request.UserPrompt.Length);

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("AI provider response received. Length: {Len}", jsonResponse.Length);

        using var doc = JsonDocument.Parse(jsonResponse);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";

        return new McpAiResult
        {
            Content = content,
            ModelUsed = model
        };
    }
}
