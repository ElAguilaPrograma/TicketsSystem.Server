using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TicketsSystem.Tests.Integration.Fixtures;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly string _connectionString;

	public ApiWebApplicationFactory(string connectionString)
	{
		_connectionString = connectionString;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");

		builder.ConfigureAppConfiguration((_, config) =>
		{
			var settings = new Dictionary<string, string?>
			{
				["ConnectionStrings:DefaultConnection"] = _connectionString,
				["Jwt:Key"] = "ThisIsAStrongTestingJwtKey1234567890",
				["Jwt:Issuer"] = "TicketsSystem.Tests",
				["Jwt:Audience"] = "TicketsSystem.Tests.Client"
			};

			config.AddInMemoryCollection(settings);
		});
	}
}