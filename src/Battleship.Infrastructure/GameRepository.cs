using Battleship.Domain;
using Microsoft.EntityFrameworkCore;

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
}