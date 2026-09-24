
using Battleship.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Battleship.Api.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public string ConnectionString => _postgreSqlContainer.GetConnectionString();

    public BattleshipDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<BattleshipDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);

    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}
