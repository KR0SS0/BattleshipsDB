namespace Battleship.Domain.Tests;

public class GameTests
{
    private static void PlaceFullFleet(Board board)
    {
        board.PlaceShip(new Ship(ShipType.Carrier, new Coordinate(0, 0), Orientation.Horizontal));
        board.PlaceShip(new Ship(ShipType.Battleship, new Coordinate(0, 1), Orientation.Horizontal));
        board.PlaceShip(new Ship(ShipType.Cruiser, new Coordinate(0, 2), Orientation.Horizontal));
        board.PlaceShip(new Ship(ShipType.Submarine, new Coordinate(0, 3), Orientation.Horizontal));
        board.PlaceShip(new Ship(ShipType.Destroyer, new Coordinate(0, 4), Orientation.Horizontal));
    }


    [Fact]
    public void NewGame_CurrentTurnIsPlayer()
    {
        var game = new Game();

        Assert.Equal(Side.Player, game.CurrentTurn);
    }

    [Fact]
    public void NewGame_HasNoWinnerAndIsNotOver()
    {
        var game = new Game();

        Assert.Null(game.Winner);
        Assert.False(game.IsOver);
    }

    [Fact]
    public void PlayerBoardFullySunk_OpponentWinsAndGameIsOver()
    {
        // Arrange
        var game = new Game();
        var ship = new Ship(ShipType.Destroyer, new Coordinate(0, 0), Orientation.Horizontal);
        Assert.True(game.PlayerBoard.PlaceShip(ship).IsSuccess);

        // Act
        game.PlayerBoard.ReceiveShot(new Coordinate(0, 0));
        game.PlayerBoard.ReceiveShot(new Coordinate(1, 0));

        // Assert
        Assert.Equal(Side.Opponent, game.Winner);
        Assert.True(game.IsOver);
    }

    [Fact]
    public void OpponentBoardFullySunk_PlayerWinsAndGameIsOver()
    {
        // Arrange
        var game = new Game();
        var ship = new Ship(ShipType.Destroyer, new Coordinate(0, 0), Orientation.Horizontal);
        Assert.True(game.OpponentBoard.PlaceShip(ship).IsSuccess);

        // Act
        game.OpponentBoard.ReceiveShot(new Coordinate(0, 0));
        game.OpponentBoard.ReceiveShot(new Coordinate(1, 0));

        // Assert
        Assert.Equal(Side.Player, game.Winner);
        Assert.True(game.IsOver);
    }

    [Fact]
    public void PlayerBoardPartiallyHit_NoWinnerYet()
    {
        var game = new Game();
        var ship = new Ship(ShipType.Destroyer, new Coordinate(0, 0), Orientation.Horizontal);
        Assert.True(game.PlayerBoard.PlaceShip(ship).IsSuccess);

        game.PlayerBoard.ReceiveShot(new Coordinate(0, 0));   // only one of two cells

        Assert.Null(game.Winner);
        Assert.False(game.IsOver);
    }

    [Fact]
    public void Shoot_ShipsNotYetPlaced_FailsWithShipsNotPlaced()
    {
        var game = new Game();

        var result = game.Shoot(new Coordinate(0, 0));

        Assert.False(result.IsSuccess);
        Assert.Equal(GameErrors.ShipsNotPlaced, result.Error);
    }

    [Fact]
    public void Shoot_PlayerFires_HitsOpponentBoardAndFlipsTurn()
    {
        var game = new Game();
        PlaceFullFleet(game.PlayerBoard);
        PlaceFullFleet(game.OpponentBoard);

        var result = game.Shoot(new Coordinate(0, 0));   // hits OpponentBoard's Carrier

        Assert.True(result.IsSuccess);
        Assert.Equal(ShotOutcome.Hit, result.Value.Outcome);
        Assert.Equal(Side.Opponent, game.CurrentTurn);
    }

    [Fact]
    public void Shoot_SecondShot_TargetsPlayerBoard()
    {
        var game = new Game();
        PlaceFullFleet(game.PlayerBoard);
        PlaceFullFleet(game.OpponentBoard);

        game.Shoot(new Coordinate(9, 9));                 // Player's turn, misses
        var result = game.Shoot(new Coordinate(0, 0));    // now Opponent's turn, hits PlayerBoard's Carrier

        Assert.Equal(ShotOutcome.Hit, result.Value.Outcome);
        Assert.Equal(Side.Player, game.CurrentTurn);
    }

    [Fact]
    public void Shoot_SameCoordinateAgainOnSameBoard_FailsAndTurnDoesNotFlip()
    {
        var game = new Game();
        PlaceFullFleet(game.PlayerBoard);
        PlaceFullFleet(game.OpponentBoard);

        game.Shoot(new Coordinate(5, 5));   // Player shoots OpponentBoard, succeeds, turn -> Opponent
        game.Shoot(new Coordinate(9, 9));   // Opponent shoots PlayerBoard, succeeds, turn -> Player
        var result = game.Shoot(new Coordinate(5, 5));   // Player shoots OpponentBoard AGAIN

        Assert.False(result.IsSuccess);
        Assert.Equal(Side.Player, game.CurrentTurn);   // didn't flip, because this shot failed
    }

    [Fact]
    public void PlayRound_ValidCell_BothSidesShootAndTurnReturnsToPlayer()
    {
        // Arrange
        var game = new Game();
        PlaceFullFleet(game.PlayerBoard);
        PlaceFullFleet(game.OpponentBoard);
        var target = new Coordinate(0, 0);

        // Act
        var result = game.PlayRound(target, new Random(1));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(target, result.Value.PlayerShot.Coordinate);
        Assert.NotNull(result.Value.OpponentShot);
        Assert.Single(game.OpponentBoard.ShotHistory);
        Assert.Single(game.PlayerBoard.ShotHistory);
        Assert.Equal(Side.Player, game.CurrentTurn);
    }

    [Fact]
    public void PlayRound_AlreadyShotCell_FailsAndOpponentDoesNotShoot()
    {
        // Arrange
        var game = new Game();
        PlaceFullFleet(game.PlayerBoard);
        PlaceFullFleet(game.OpponentBoard);
        var target = new Coordinate(5, 5);
        game.PlayRound(target, new Random(1));

        // Act
        var result = game.PlayRound(target, new Random(1));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ShotErrors.AlreadyShotSpace, result.Error);
        Assert.Single(game.PlayerBoard.ShotHistory);
        Assert.Equal(Side.Player, game.CurrentTurn);
    }

    [Fact]
    public void PlayRound_WinningShot_OpponentDoesNotReply()
    {
        // Arrange
        var game = new Game();
        PlaceFullFleet(game.PlayerBoard);
        PlaceFullFleet(game.OpponentBoard);

        // Act
        var lastRound = SinkOpponentFleet(game);

        // Assert
        Assert.Null(lastRound.OpponentShot);
        Assert.Equal(ShotOutcome.Sunk, lastRound.PlayerShot.Outcome);
        Assert.Equal(Side.Player, game.Winner);
    }

    [Fact]
    public void PlayRound_GameIsOver_FailsWithGameOver()
    {
        // Arrange
        var game = new Game();
        PlaceFullFleet(game.PlayerBoard);
        PlaceFullFleet(game.OpponentBoard);
        SinkOpponentFleet(game);

        // Act
        var result = game.PlayRound(new Coordinate(9, 9), new Random(1));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GameErrors.GameOver, result.Error);
    }

    [Fact]
    public void PlayRound_NotYourTurn_Fails()
    {
        // Arrange
        var game = new Game();
        PlaceFullFleet(game.PlayerBoard);
        PlaceFullFleet(game.OpponentBoard);

        // Act
        game.Shoot(new Coordinate(0, 0));
        var result = game.PlayRound(new Coordinate(1, 0), new Random());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GameErrors.NotYourTurn, result.Error);
    }

    [Fact]
    public void PlayRound_WinningShot_ReturnsNullOpponentShot()
    {
        // Arrange
        var game = new Game();
        PlaceFullFleet(game.PlayerBoard);
        PlaceFullFleet(game.OpponentBoard);

        // Act
        game.Shoot(new Coordinate(0, 0));
        var result = game.PlayRound(new Coordinate(1, 0), new Random());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(GameErrors.NotYourTurn, result.Error);
    }

    private static Round SinkOpponentFleet(Game game)
    {
        var random = new Random(1);
        Round? lastRound = null;

        foreach (var cell in game.OpponentBoard.Ships.SelectMany(ship => ship.GetCells()))
        {
            var result = game.PlayRound(cell, random);
            Assert.True(result.IsSuccess);
            lastRound = result.Value;
        }

        Assert.NotNull(lastRound);
        return lastRound;
    }
}

