
using Battleship.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Testcontainers.PostgreSql;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public string ConnectionString => _postgreSqlContainer.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var dbContextFactory = new PooledDbContextFactory<BattleshipDbContext>(
             new DbContextOptionsBuilder<BattleshipDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);

        using var context = dbContextFactory.CreateDbContext();

        await context.Database.MigrateAsync();
    }
    public async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}


