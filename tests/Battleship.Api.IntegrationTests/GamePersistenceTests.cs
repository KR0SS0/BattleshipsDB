using Battleship.Domain;
using Battleship.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Battleship.Api.IntegrationTests;

public sealed class GamePersistenceTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async ValueTask SaveAndReload_IsSameGameInEntityAndDomain()
    {
        // Arrange
        var game = new Game();
        Assert.True(game.PlayerBoard.TryPlaceShipsRandomly(new Random(1)));
        Assert.True(game.OpponentBoard.TryPlaceShipsRandomly(new Random(2)));

        Coordinate[] targets = [new(0, 0), new(1, 1), new(2, 2), new(3, 3), new(4, 4)];
        var shotEntities = new List<ShotEntity>();

        for (var i = 0; i < targets.Length; i++)
        {
            var targetSide = game.CurrentTurn == Side.Player ? Side.Opponent : Side.Player;
            var result = game.Shoot(targets[i]);
            Assert.True(result.IsSuccess);

            shotEntities.Add(GameMapper.ToShotEntity(result.Value, targetSide, targets[i], sequence: i + 1));
        }

        var entity = GameMapper.ToGameEntity(game);
        entity.Shots.AddRange(shotEntities);

        // Act
        await using (var saveContext = fixture.CreateContext())
        {
            saveContext.Games.Add(entity);
            await saveContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var loadContext = fixture.CreateContext();
        var loaded = await loadContext.Games
            .Include(g => g.Ships)
            .Include(g => g.Shots)
            .SingleAsync(g => g.Id == game.Id, TestContext.Current.CancellationToken);

        var gameDomain = GameMapper.ToGameDomain(loaded);

        // Assert
        Assert.Equal(targets.Length, loaded.Shots.Count);
        Assert.Equal(game.Id, gameDomain.Id);
        Assert.Equal(game.CurrentTurn, gameDomain.CurrentTurn);
        Assert.Equal(
            game.PlayerBoard.Ships.OrderBy(s => s.Type),
            gameDomain.PlayerBoard.Ships.OrderBy(s => s.Type));
        Assert.Equal(
            game.OpponentBoard.Ships.OrderBy(s => s.Type),
            gameDomain.OpponentBoard.Ships.OrderBy(s => s.Type));
    }

    [Fact]
    public async ValueTask LoadGameAsync_UnknownId_ReturnsNull()
    {
        // Arrange
        await using var context = fixture.CreateContext();
        var gameRepository = new GameRepository(context);

        // Act
        Game? loadedGame = await gameRepository.LoadGameAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(loadedGame);
    }
    
    [Fact]
    public async ValueTask SaveGameAsync_Reloading_ReturnsSavedGame()
    {
        // Arrange
        Game game = new();
        Assert.True(game.PlayerBoard.TryPlaceShipsRandomly(new Random(0)));
        Assert.True(game.OpponentBoard.TryPlaceShipsRandomly(new Random(1)));

        // Act
        await using (var saveContext = fixture.CreateContext())
        {
            var saveRepository = new GameRepository(saveContext);
            await saveRepository.SaveGameAsync(game, TestContext.Current.CancellationToken);
        }

        await using var loadContext = fixture.CreateContext();
        var loadRepository = new GameRepository(loadContext);
        var loadedGame = await loadRepository.LoadGameAsync(game.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(loadedGame);
        Assert.Equal(game.Id, loadedGame.Id);
        Assert.Equal(game.CurrentTurn, loadedGame.CurrentTurn);
        Assert.Equal(
            game.PlayerBoard.Ships.OrderBy(s => s.Type),
            loadedGame.PlayerBoard.Ships.OrderBy(s => s.Type));
        Assert.Equal(
            game.OpponentBoard.Ships.OrderBy(s => s.Type),
            loadedGame.OpponentBoard.Ships.OrderBy(s => s.Type));
    }
}