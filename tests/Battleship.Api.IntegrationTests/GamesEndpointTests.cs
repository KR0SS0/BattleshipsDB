using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Battleship.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Battleship.Api.IntegrationTests;

// IClassFixture<ApiFixture> gives every test in this class the same running API + database.
public sealed class GamesEndpointTests(ApiFixture api) : IClassFixture<ApiFixture>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async ValueTask PostGames_Returns201WithGameIdAndLocation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = api.CreateClient();

        // Act
        var response = await client.PostAsync("/games", content: null, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GameResponse>(JsonOptions, cancellationToken);
        Assert.NotNull(body);
        Assert.Equal($"/games/{body.GameId}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async ValueTask GetGame_NewGame_ShowsOwnFleetButNoEnemyShips()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = api.CreateClient();
        var gameId = await CreateGameAsync(client, cancellationToken);

        // Act
        var response = await client.GetAsync($"/games/{gameId}", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var game = await response.Content.ReadFromJsonAsync<GameStateResponse>(JsonOptions, cancellationToken);
        Assert.NotNull(game);
        Assert.Equal(Side.Player, game.Turn);
        Assert.Equal(5, game.PlayerBoard.Ships.Count);
        Assert.Empty(game.OpponentBoard.SunkShips);
        Assert.Empty(game.OpponentBoard.ShotsReceived);
    }

    [Fact]
    public async ValueTask GetGame_UnknownId_Returns404()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = api.CreateClient();

        // Act
        var response = await client.GetAsync($"/games/{Guid.NewGuid()}", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async ValueTask FireShot_ValidCell_Returns200WithBothShots()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = api.CreateClient();
        var gameId = await CreateGameAsync(client, cancellationToken);

        // Act
        var response = await client.PostAsJsonAsync(
            $"/games/{gameId}/shots", new FireShotRequest("B7"), cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FireShotResponse>(JsonOptions, cancellationToken);
        Assert.NotNull(body);
        Assert.Equal("B7", body.PlayerShot.Coordinate);
        Assert.NotNull(body.OpponentShot);
        Assert.Equal(Side.Player, body.Game.Turn);          

        // Each shot shows up on the board it landed on.
        Assert.Equal("B7", Assert.Single(body.Game.OpponentBoard.ShotsReceived).Coordinate);
        Assert.Equal(body.OpponentShot.Coordinate, Assert.Single(body.Game.PlayerBoard.ShotsReceived).Coordinate);
    }

    [Fact]
    public async ValueTask FireShot_SameCellTwice_SecondReturns409WithCode()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = api.CreateClient();
        var gameId = await CreateGameAsync(client, cancellationToken);
        var first = await client.PostAsJsonAsync(
            $"/games/{gameId}/shots", new FireShotRequest("B7"), cancellationToken);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        // Act
        var second = await client.PostAsJsonAsync(
            $"/games/{gameId}/shots", new FireShotRequest("B7"), cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("shot.already_shot_space", await ReadErrorCodeAsync(second, cancellationToken));
    }

    [Fact]
    public async ValueTask FireShot_UnparseableCell_Returns400()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = api.CreateClient();
        var gameId = await CreateGameAsync(client, cancellationToken);

        // Act
        var response = await client.PostAsJsonAsync(
            $"/games/{gameId}/shots", new FireShotRequest("Z99x"), cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async ValueTask FireShot_OffBoardCell_Returns422WithCode()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = api.CreateClient();
        var gameId = await CreateGameAsync(client, cancellationToken);

        // Act
        var response = await client.PostAsJsonAsync(
            $"/games/{gameId}/shots", new FireShotRequest("K5"), cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("shot.out_of_bounds", await ReadErrorCodeAsync(response, cancellationToken));
    }

    [Fact]
    public async ValueTask FireShot_UnknownGame_Returns404()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = api.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            $"/games/{Guid.NewGuid()}/shots", new FireShotRequest("A1"), cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Create a game for tests that needs it.
    private static async Task<Guid> CreateGameAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var response = await client.PostAsync("/games", content: null, cancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GameResponse>(JsonOptions, cancellationToken);
        Assert.NotNull(body);
        return body.GameId;
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions, cancellationToken);
        Assert.NotNull(problem);
        return problem.Extensions["code"]?.ToString();
    }
}
