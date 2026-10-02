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

    internal Game(Guid id, Difficulty difficulty)
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

    //A round where player shoots then opponent shoots back.
    public Result<Round> PlayRound(Coordinate playerCellTarget, Random random)
    {
        var playerShot = PlayerTurn(playerCellTarget);
        if (!playerShot.IsSuccess)
            return Result<Round>.Failure(playerShot.Error);

        return Result<Round>.Success(new Round(playerShot.Value, OpponentTurn(random)));
    }

    // The first half of a round. Afterwards it's the opponent's turn, unless the player just won.
    public Result<Shot> PlayerTurn(Coordinate target)
    {
        if (IsOver)
            return Result<Shot>.Failure(GameErrors.GameOver);
        if (CurrentTurn != Side.Player)
            return Result<Shot>.Failure(GameErrors.NotYourTurn);

        return Shoot(target);
    }

    // The second half of a round. Null when the game is already over.
    public Shot? OpponentTurn(Random random)
    {
        if (IsOver)
            return null;
        if (CurrentTurn != Side.Opponent)
            throw new InvalidOperationException("It is not the opponent's turn.");

        var target = Difficulty.ShotStrategy().ChooseTarget(PlayerBoard.ShotHistory, random);
        return target is null ? null : Shoot(target.Value).Value;
    }
}