using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using TicketsSystem.Data;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Tests.Integration.Fixtures;

public sealed class SqlServerContainerFixture : IAsyncLifetime
{
	public static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
	public static readonly Guid AgentUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
	public static readonly Guid EndUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

	public const string AdminEmail = "admin.integration@test.com";
	public const string AgentEmail = "agent.integration@test.com";
	public const string EndUserEmail = "user.integration@test.com";
	public const string SharedPassword = "IntegrationPass123!";

	private MsSqlContainer? _container;

	public string ConnectionString => _container?.GetConnectionString() ?? string.Empty;

	public async Task InitializeAsync()
	{
		_container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
			.WithPassword("Your_strong_Password_123")
			.WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(1433))
			.Build();

		await _container.StartAsync();

		var options = new DbContextOptionsBuilder<SystemTicketsContext>()
			.UseSqlServer(ConnectionString)
			.Options;

		await using var dbContext = new SystemTicketsContext(options);
      await dbContext.Database.MigrateAsync();
		await SeedReferenceDataAsync(dbContext);
		await SeedUsersAsync(dbContext);
	}

	public async Task DisposeAsync()
	{
		if (_container != null)
		{
			await _container.DisposeAsync();
		}
	}

	public SystemTicketsContext CreateDbContext()
	{
		var options = new DbContextOptionsBuilder<SystemTicketsContext>()
			.UseSqlServer(ConnectionString)
			.Options;

		return new SystemTicketsContext(options);
	}

	private static async Task SeedReferenceDataAsync(SystemTicketsContext dbContext)
	{
		if (!await dbContext.TicketStatuses.AnyAsync())
		{
			dbContext.TicketStatuses.AddRange(
				new TicketStatus { Name = "Open" },
				new TicketStatus { Name = "InProgress" },
				new TicketStatus { Name = "OnHold" },
				new TicketStatus { Name = "Closed" },
				new TicketStatus { Name = "Reopened" });
		}

		if (!await dbContext.TicketPriorities.AnyAsync())
		{
			dbContext.TicketPriorities.AddRange(
				new TicketPriority { Name = "Low", Level = 1 },
				new TicketPriority { Name = "Medium", Level = 2 },
				new TicketPriority { Name = "High", Level = 3 },
				new TicketPriority { Name = "Critical", Level = 4 });
		}

		await dbContext.SaveChangesAsync();
	}

	private static async Task SeedUsersAsync(SystemTicketsContext dbContext)
	{
		var hasher = new PasswordHasher<User>();

		if (!await dbContext.Users.AnyAsync(u => u.UserId == AdminUserId))
		{
			var admin = new User
			{
				UserId = AdminUserId,
				FullName = "Integration Admin",
				Email = AdminEmail,
				Role = "Admin",
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			};
			admin.PasswordHash = hasher.HashPassword(admin, SharedPassword);
			dbContext.Users.Add(admin);
		}

		if (!await dbContext.Users.AnyAsync(u => u.UserId == AgentUserId))
		{
			var agent = new User
			{
				UserId = AgentUserId,
				FullName = "Integration Agent",
				Email = AgentEmail,
				Role = "Agent",
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			};
			agent.PasswordHash = hasher.HashPassword(agent, SharedPassword);
			dbContext.Users.Add(agent);
		}

		if (!await dbContext.Users.AnyAsync(u => u.UserId == EndUserId))
		{
			var endUser = new User
			{
				UserId = EndUserId,
				FullName = "Integration User",
				Email = EndUserEmail,
				Role = "User",
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			};
			endUser.PasswordHash = hasher.HashPassword(endUser, SharedPassword);
			dbContext.Users.Add(endUser);
		}

		await dbContext.SaveChangesAsync();
	}
}