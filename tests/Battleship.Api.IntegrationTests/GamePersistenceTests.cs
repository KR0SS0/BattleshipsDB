using Battleship.Domain;
using Battleship.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Battleship.Api.IntegrationTests;

public sealed class GamePersistenceTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    [Fact]
    public async ValueTask SaveAndReload_IsSameGame()
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

        var reloaded = GameMapper.ToGameDomain(loaded);

        // Assert
        Assert.Equal(targets.Length, loaded.Shots.Count);
        Assert.Equal(game.Id, reloaded.Id);
        Assert.Equal(game.CurrentTurn, reloaded.CurrentTurn);
        Assert.Equal(
            game.PlayerBoard.Ships.OrderBy(s => s.Type),
            reloaded.PlayerBoard.Ships.OrderBy(s => s.Type));
        Assert.Equal(
            game.OpponentBoard.Ships.OrderBy(s => s.Type),
            reloaded.OpponentBoard.Ships.OrderBy(s => s.Type));
    }
}
