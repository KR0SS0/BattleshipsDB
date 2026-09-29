using Microsoft.AspNetCore.Mvc.Testing;

namespace Battleship.Api.IntegrationTests;

// Database + running API
public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgresFixture _database = new();

    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient CreateClient() => _factory.CreateClient();

    public async ValueTask InitializeAsync()
    {
        // Start the container and apply migrations.
        await _database.InitializeAsync();

        //    WebApplicationFactory<Program> runs Program.cs,
        //    and WithWebHostBuilder lets the test change settings before it
        //    starts. UseSetting overrides the connection string, so the API
        //    talks to the throwaway container instead of the dev database.
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.UseSetting("ConnectionStrings:BattleShipDbContext", _database.ConnectionString));
    }

    public async ValueTask DisposeAsync()
    {
        // Stop api then database
        await _factory.DisposeAsync();
        await _database.DisposeAsync();
    }
}
