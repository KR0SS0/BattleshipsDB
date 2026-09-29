using Battleship.Domain;

namespace Battleship.Infrastructure;

public static class PersistenceErrors
{
    public static readonly Error ConcurrentUpdate = new(ErrorType.Conflict, "game.concurrent_update",
        "A request already changed the game. Try again.");
}