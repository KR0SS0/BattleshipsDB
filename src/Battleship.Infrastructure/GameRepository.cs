using Battleship.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Battleship.Infrastructure;

public sealed class GameRepository(BattleshipDbContext context)
{
    public async Task SaveGameAsync(Game game, CancellationToken cancellationToken)
    {
        var gameEntity = GameMapper.ToGameEntity(game);
        context.Games.Add(gameEntity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Game?> LoadGameAsync(Guid id, CancellationToken cancellationToken)
    {
        var gameEntity = await context.Games
            .Include(g => g.Ships)
            .Include(g => g.Shots)
            .SingleOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (gameEntity is null) return null;
        return GameMapper.ToGameDomain(gameEntity);
    }

    public void AddShot(Game game, Shot shot, Side targetSide)
    {
        var gameEntity = context.Games.Local.SingleOrDefault(g => g.Id == game.Id);
        if (gameEntity is null)
        {
            throw new InvalidOperationException(
                $"Game {game.Id} must be loaded or saved through this repository before saving a shot.");
        }

        gameEntity.CurrentTurn = game.CurrentTurn;
        gameEntity.Shots.Add(GameMapper.ToShotEntity(shot, targetSide,
            sequence: gameEntity.Shots.Count + 1));
    }

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(PersistenceErrors.ConcurrentUpdate);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Result.Failure(PersistenceErrors.ConcurrentUpdate);
        }
        return Result.Success();
    }
}