using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using TicketsSystem.Core.DTOs.TicketsCommentsDTO;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Tests.Integration.Fixtures;

namespace TicketsSystem.Tests.Integration.Integration;

[Collection(IntegrationTestCollection.Name)]
public sealed class SignalRIntegrationTests : IAsyncLifetime
{
    private readonly SqlServerContainerFixture _dbFixture;
    private ApiWebApplicationFactory? _factory;
    private HttpClient? _client;

    public SignalRIntegrationTests(SqlServerContainerFixture dbFixture)
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
    public async Task AgentReceives_ReceiveNewTicket_WhenUserCreatesTicket()
    {
        var agentToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.AgentEmail, SqlServerContainerFixture.SharedPassword);
        var userToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.EndUserEmail, SqlServerContainerFixture.SharedPassword);

        var receivedTicketTcs = new TaskCompletionSource<TicketsReadDto>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var hubConnection = BuildHubConnection(agentToken);
        hubConnection.On<TicketsReadDto>("ReceiveNewTicket", ticket =>
        {
            receivedTicketTcs.TrySetResult(ticket);
        });

        await hubConnection.StartAsync();

        var title = $"SR-CREATE-{Guid.NewGuid():N}";
        var createBody = new
        {
            title,
            description = "SignalR create event test",
            priorityId = 2
        };

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/tickets/createticket")
        {
            Content = JsonContent.Create(createBody)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var createResponse = await _client!.SendAsync(createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var receivedTicket = await receivedTicketTcs.Task.WaitAsync(TimeSpan.FromSeconds(15));
        receivedTicket.Title.Should().Be(title);
        receivedTicket.CreatedByUserId.Should().Be(SqlServerContainerFixture.EndUserId);
    }

    [Fact]
    public async Task UserReceives_ReceiveNewTicketStatusChange_WhenAgentClosesTicket()
    {
        var userToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.EndUserEmail, SqlServerContainerFixture.SharedPassword);
        var agentToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.AgentEmail, SqlServerContainerFixture.SharedPassword);

        var receivedStatusChangeTcs = new TaskCompletionSource<TicketsReadDto>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var hubConnection = BuildHubConnection(userToken);
        hubConnection.On<TicketsReadDto>("ReceiveNewTicketStatusChange", ticket =>
        {
            receivedStatusChangeTcs.TrySetResult(ticket);
        });

        await hubConnection.StartAsync();

        var title = $"SR-CLOSE-{Guid.NewGuid():N}";
        var createBody = new
        {
            title,
            description = "SignalR close event test",
            priorityId = 1
        };

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/tickets/createticket")
        {
            Content = JsonContent.Create(createBody)
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var createResponse = await _client!.SendAsync(createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var lookupContext = _dbFixture.CreateDbContext();
        var createdTicket = await lookupContext.Tickets
            .AsNoTracking()
            .Where(t => t.Title == title)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        createdTicket.Should().NotBeNull();

        using var closeRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/closetickets/{createdTicket!.TicketId}");
        closeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", agentToken);

        var closeResponse = await _client.SendAsync(closeRequest);
        closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusChangedTicket = await receivedStatusChangeTcs.Task.WaitAsync(TimeSpan.FromSeconds(15));
        statusChangedTicket.TicketId.Should().Be(createdTicket.TicketId);
        statusChangedTicket.Title.Should().Be(title);
        statusChangedTicket.StatusId.Should().Be(4);
    }

    [Fact]
    public async Task ExternalComment_ByAgent_NotifiesUserOnly()
    {
        var userToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.EndUserEmail, SqlServerContainerFixture.SharedPassword);
        var agentToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.AgentEmail, SqlServerContainerFixture.SharedPassword);

        var userCommentTcs = new TaskCompletionSource<TicketsReadComment>(TaskCreationOptions.RunContinuationsAsynchronously);
        var agentCommentTcs = new TaskCompletionSource<TicketsReadComment>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var userHub = BuildHubConnection(userToken);
        await using var agentHub = BuildHubConnection(agentToken);

        userHub.On<TicketsReadComment>("ReceiveNewTicketComment", comment => userCommentTcs.TrySetResult(comment));
        agentHub.On<TicketsReadComment>("ReceiveNewTicketComment", comment => agentCommentTcs.TrySetResult(comment));

        await userHub.StartAsync();
        await agentHub.StartAsync();

        var ticketId = await CreateAndAssignTicketAsync(userToken, agentToken, "SR-CMT-A");
        var commentText = $"agent-visible-comment-{Guid.NewGuid():N}";

        var createCommentResponse = await CreateCommentAsync(agentToken, ticketId, commentText, isInternal: false);
        createCommentResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.NoContent);

        var userReceived = await userCommentTcs.Task.WaitAsync(TimeSpan.FromSeconds(15));
        userReceived.TicketId.Should().Be(ticketId);
        userReceived.Content.Should().Be(commentText);
        userReceived.IsInternal.Should().BeFalse();
        userReceived.UserId.Should().Be(SqlServerContainerFixture.AgentUserId);

        var agentReceived = await WaitForOptionalEventAsync(agentCommentTcs.Task, TimeSpan.FromSeconds(3));
        agentReceived.Should().BeNull();
    }

    [Fact]
    public async Task ExternalComment_ByUser_NotifiesAgentOnly()
    {
        var userToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.EndUserEmail, SqlServerContainerFixture.SharedPassword);
        var agentToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.AgentEmail, SqlServerContainerFixture.SharedPassword);

        var userCommentTcs = new TaskCompletionSource<TicketsReadComment>(TaskCreationOptions.RunContinuationsAsynchronously);
        var agentCommentTcs = new TaskCompletionSource<TicketsReadComment>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var userHub = BuildHubConnection(userToken);
        await using var agentHub = BuildHubConnection(agentToken);

        userHub.On<TicketsReadComment>("ReceiveNewTicketComment", comment => userCommentTcs.TrySetResult(comment));
        agentHub.On<TicketsReadComment>("ReceiveNewTicketComment", comment => agentCommentTcs.TrySetResult(comment));

        await userHub.StartAsync();
        await agentHub.StartAsync();

        var ticketId = await CreateAndAssignTicketAsync(userToken, agentToken, "SR-CMT-U");
        var commentText = $"user-visible-comment-{Guid.NewGuid():N}";

        var createCommentResponse = await CreateCommentAsync(userToken, ticketId, commentText, isInternal: false);
        createCommentResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.NoContent);

        var agentReceived = await agentCommentTcs.Task.WaitAsync(TimeSpan.FromSeconds(15));
        agentReceived.TicketId.Should().Be(ticketId);
        agentReceived.Content.Should().Be(commentText);
        agentReceived.IsInternal.Should().BeFalse();
        agentReceived.UserId.Should().Be(SqlServerContainerFixture.EndUserId);

        var userReceived = await WaitForOptionalEventAsync(userCommentTcs.Task, TimeSpan.FromSeconds(3));
        userReceived.Should().BeNull();
    }

    [Fact]
    public async Task InternalComment_ByAgent_DoesNotNotifyUser()
    {
        var userToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.EndUserEmail, SqlServerContainerFixture.SharedPassword);
        var agentToken = await LoginAndGetJwtAsync(SqlServerContainerFixture.AgentEmail, SqlServerContainerFixture.SharedPassword);

        var userCommentTcs = new TaskCompletionSource<TicketsReadComment>(TaskCreationOptions.RunContinuationsAsynchronously);
        var agentCommentTcs = new TaskCompletionSource<TicketsReadComment>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var userHub = BuildHubConnection(userToken);
        await using var agentHub = BuildHubConnection(agentToken);

        userHub.On<TicketsReadComment>("ReceiveNewTicketComment", comment => userCommentTcs.TrySetResult(comment));
        agentHub.On<TicketsReadComment>("ReceiveNewTicketComment", comment => agentCommentTcs.TrySetResult(comment));

        await userHub.StartAsync();
        await agentHub.StartAsync();

        var ticketId = await CreateAndAssignTicketAsync(userToken, agentToken, "SR-CMT-I");

        var createCommentResponse = await CreateCommentAsync(agentToken, ticketId,
            $"agent-internal-comment-{Guid.NewGuid():N}", isInternal: true);
        createCommentResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.NoContent);

        var userReceived = await WaitForOptionalEventAsync(userCommentTcs.Task, TimeSpan.FromSeconds(4));
        userReceived.Should().BeNull();

        var agentReceived = await WaitForOptionalEventAsync(agentCommentTcs.Task, TimeSpan.FromSeconds(2));
        agentReceived.Should().BeNull();
    }

    private async Task<Guid> CreateAndAssignTicketAsync(string userToken, string agentToken, string titlePrefix)
    {
        var title = $"{titlePrefix}-{Guid.NewGuid():N}";
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/tickets/createticket")
        {
            Content = JsonContent.Create(new
            {
                title,
                description = "SignalR comments ticket",
                priorityId = 2
            })
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        var createResponse = await _client!.SendAsync(createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var lookupContext = _dbFixture.CreateDbContext();
        var ticket = await lookupContext.Tickets
            .AsNoTracking()
            .Where(t => t.Title == title)
            .OrderByDescending(t => t.CreatedAt)
            .FirstAsync();

        using var acceptRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/tickets/accepttickets/{ticket.TicketId}");
        acceptRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", agentToken);
        var acceptResponse = await _client.SendAsync(acceptRequest);
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        return ticket.TicketId;
    }

    private async Task<HttpResponseMessage> CreateCommentAsync(string token, Guid ticketId, string content, bool isInternal)
    {
        using var commentRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/ticketcomments/createticketcomment/{ticketId}")
        {
            Content = JsonContent.Create(new
            {
                content,
                isInternal
            })
        };
        commentRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _client!.SendAsync(commentRequest);
    }

    private static async Task<T?> WaitForOptionalEventAsync<T>(Task<T> task, TimeSpan timeout) where T : class
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        if (completedTask == task)
        {
            return await task;
        }

        return null;
    }

    private HubConnection BuildHubConnection(string bearerToken)
    {
        return new HubConnectionBuilder()
            .WithUrl(new Uri(_client!.BaseAddress!, "/ticketHub"), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.AccessTokenProvider = () => Task.FromResult<string?>(bearerToken);
                options.HttpMessageHandlerFactory = _ => _factory!.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();
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
