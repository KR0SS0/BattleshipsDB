namespace Battleship.Domain;

public sealed class Game
{
    public Board PlayerBoard { get; } = new();
    public Board OpponentBoard { get; } = new();
    public Side CurrentTurn { get; private set; } = Side.Player;
    public Guid Id { get; } = Guid.NewGuid();
    public Difficulty Difficulty { get; }
    public const Difficulty DefaultDifficulty = Difficulty.Normal;

    public Game(Difficulty difficulty = DefaultDifficulty)
    {
        Difficulty = difficulty;
    }
    
    internal Game(Guid id, Difficulty difficulty = DefaultDifficulty)
    {
        Id = id;
        Difficulty = difficulty;
    }

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

        var targetBoard = CurrentTurn == Side.Player ? OpponentBoard : PlayerBoard;
        var result = targetBoard.ReceiveShot(coordinate);

        // Switch whose turn it is  
        if (result.IsSuccess)
            CurrentTurn = CurrentTurn == Side.Player ? Side.Opponent : Side.Player;

        return result;
    }
}