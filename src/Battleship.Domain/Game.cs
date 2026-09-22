namespace Battleship.Domain;

public sealed class Game
{
    public Board PlayerBoard { get; } = new();
    public Board OpponentBoard { get; } = new();
    public Side CurrentTurn { get; private set; } = Side.Player;

    public Side? Winner =>
        PlayerBoard.AreAllShipsSunk ? Side.Opponent :
        OpponentBoard.AreAllShipsSunk ? Side.Player :
        null;

    public bool IsOver => Winner is not null;

    public Result<Shot> Shoot(Coordinate coordinate)
    {
        if (IsOver)
            return Result<Shot>.Failure(GameErrors.GameOver);
        if (!PlayerBoard.AreAllShipsPlaced || !OpponentBoard.AreAllShipsPlaced)
            return Result<Shot>.Failure(GameErrors.ShipsNotPlaced);

        throw new NotImplementedException();
    }



}