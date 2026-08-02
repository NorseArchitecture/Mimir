using System.Diagnostics.CodeAnalysis;
using Testcontainers.PostgreSql;

namespace Norse.Reference.Web.Server.Tests;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
	readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:19beta2")
		.WithDatabase("norse_reference")
		.Build();

	// null! justified: hydrated by InitializeAsync before xUnit hands the fixture to any test.
	public string ConnectionString { get; private set; } = null!;

	public async ValueTask InitializeAsync()
	{
		await _container.StartAsync();
		ConnectionString = _container.GetConnectionString();
	}

	public ValueTask DisposeAsync() =>
		_container.DisposeAsync();
}

[CollectionDefinition("Postgres")]
[SuppressMessage("Design", "CA1711:Identifiers should not have incorrect suffix", Justification = "xUnit collection fixture naming convention")]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>;
