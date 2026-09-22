namespace Battleship.Domain;

public static class GameErrors
{
    public static readonly Error GameOver = new(ErrorType.Conflict, "game.is_over", "Game is over.");
    public static readonly Error ShipsNotPlaced = new(ErrorType.Conflict, "game.ships_not_placed",
        "Not all Ships have been placed on the board.");
}