using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TicketsSystem.Tests.Integration.Fixtures;

namespace TicketsSystem.Tests.Integration.Integration;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthenticationIntegrationTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _dbFixture;
    private ApiWebApplicationFactory? _factory;
    private HttpClient? _client;

    public AuthenticationIntegrationTests(SqlServerContainerFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    public Task InitializeAsync()
    {
        _factory = new ApiWebApplicationFactory(_dbFixture.ConnectionString);
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Login_ReturnsJwt_And_CanAccessProtectedEndpoint()
    {
        var token = await LoginAndGetJwtAsync(SqlServerContainerFixture.EndUserEmail, SqlServerContainerFixture.SharedPassword);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/authentication/getcurrentuser");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client!.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var payload = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = payload.RootElement;

        Guid.Parse(root.GetProperty("userId").GetString()!).Should().Be(SqlServerContainerFixture.EndUserId);
        root.GetProperty("email").GetString().Should().Be(SqlServerContainerFixture.EndUserEmail);
        root.GetProperty("role").GetString().Should().Be("User");
    }

    [Fact]
    public async Task ProtectedEndpoint_ReturnsUnauthorized_WithoutToken()
    {
        var response = await _client!.GetAsync("/api/tickets/gettickets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTicket_PersistsTicketInDatabase()
    {
        var token = await LoginAndGetJwtAsync(SqlServerContainerFixture.EndUserEmail, SqlServerContainerFixture.SharedPassword);
        var title = $"E2E Ticket {Guid.NewGuid()}";

        var requestBody = new
        {
            title,
            description = "Persistence validation from integration test",
            priorityId = 2
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/tickets/createticket")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client!.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var dbContext = _dbFixture.CreateDbContext();
        var createdTicket = await dbContext.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Title == title);

        createdTicket.Should().NotBeNull();
        createdTicket!.CreatedByUserId.Should().Be(SqlServerContainerFixture.EndUserId);
        createdTicket.StatusId.Should().Be(1);
        createdTicket.PriorityId.Should().Be(2);
    }

    [Fact]
    public async Task AgentAcceptAndCloseTicket_CreatesHistoryAndNotification()
    {
        var endUserToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.EndUserEmail, SqlServerContainerFixture.SharedPassword);
        var title = $"LC-{Guid.NewGuid():N}";

        var createBody = new
        {
            title,
            description = "Ticket lifecycle integration",
            priorityId = 1
        };
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/tickets/createticket")
        {
            Content = JsonContent.Create(createBody)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", endUserToken);
        var createResponse = await _client!.SendAsync(createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var lookupContext = _dbFixture.CreateDbContext();
        var createdTicket = await lookupContext.Tickets.FirstAsync(t => t.Title == title);
        var ticketId = createdTicket.TicketId;

        var agentToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.AgentEmail, SqlServerContainerFixture.SharedPassword);

        using var acceptRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/accepttickets/{ticketId}");
        acceptRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", agentToken);
        var acceptResponse = await _client.SendAsync(acceptRequest);
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var closeRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/closetickets/{ticketId}");
        closeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", agentToken);
        var closeResponse = await _client.SendAsync(closeRequest);
        closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var assertContext = _dbFixture.CreateDbContext();
        var ticketInDb = await assertContext.Tickets.AsNoTracking().FirstAsync(t => t.TicketId == ticketId);
        ticketInDb.AssignedToUserId.Should().Be(SqlServerContainerFixture.AgentUserId);
        ticketInDb.StatusId.Should().Be(4);
        ticketInDb.ClosedAt.Should().NotBeNull();

        var historyEntries = await assertContext.TicketHistories
            .AsNoTracking()
            .Where(h => h.TicketId == ticketId)
            .ToListAsync();
        historyEntries.Should().NotBeEmpty();
        historyEntries.Should().Contain(h => h.ChangedByUserId == SqlServerContainerFixture.AgentUserId);

        using var notificationsRequest = new HttpRequestMessage(HttpMethod.Get,
            $"/api/notifications/getusernotifications/{SqlServerContainerFixture.EndUserId}");
        notificationsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", endUserToken);
        var notificationsResponse = await _client.SendAsync(notificationsRequest);
        notificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var notificationsPayload = await JsonDocument.ParseAsync(await notificationsResponse.Content.ReadAsStreamAsync());
        var notifications = notificationsPayload.RootElement;
        notifications.ValueKind.Should().Be(JsonValueKind.Array);
        notifications.EnumerateArray().Any(n =>
            n.GetProperty("type").GetString() == "UpdateTicket" &&
            n.GetProperty("message").GetString()!.Contains(title, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WithInvalidCredentials()
    {
        var requestBody = new
        {
            email = "no-user@test.com",
            password = "wrong-password"
        };

        var response = await _client!.PostAsJsonAsync("/api/authentication/login", requestBody);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<string> LoginAndGetJwtAsync(string email, string password)
    {
        var loginBody = new
        {
            email,
            password
        };

        var loginResponse = await _client!.PostAsJsonAsync("/api/authentication/login", loginBody);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var authCookie = loginResponse.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(h => h.StartsWith("AuthToken=", StringComparison.OrdinalIgnoreCase));
        authCookie.Should().NotBeNull();

        var rawCookie = authCookie!;
        var tokenPart = rawCookie.Split(';', 2)[0];
        var token = tokenPart.Substring("AuthToken=".Length);

        token.Should().NotBeNullOrWhiteSpace();
        return token;
    }
}
