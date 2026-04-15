namespace TicketsSystem.Tests.Integration.Fixtures;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<SqlServerContainerFixture>
{
	public const string Name = "integration-tests";
}