namespace Battleship.Domain.Tests;

public class BoardTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 0, true)]
    [InlineData(0, 7, true)]
    [InlineData(0, 8, false)]
    [InlineData(9, 8, false)]
    [InlineData(10, 0, false)]
    [InlineData(-1, 0, false)]
    public void PlaceShip_VerticalCruiser_SucceedsOnlyWhenWholeShipFits(int column, int row, bool expected)
    {
        // Arrange
        var board = new Board();
        var ship = new Ship(ShipType.Cruiser, new Coordinate(column, row), Orientation.Vertical);

        // Act
        var result = board.PlaceShip(ship);

        // Assert
        Assert.Equal(expected, result.IsSuccess);
    }

    [Fact]
    public void PlaceShip_OutOfBounds_FailsWithOutOfBoundsAndLeavesBoardEmpty()
    {
        // Arrange
        var board = new Board();
        var ship = new Ship(ShipType.Cruiser, new Coordinate(0, 8), Orientation.Vertical);

        // Act
        var result = board.PlaceShip(ship);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PlacementErrors.OutOfBounds, result.Error);
        Assert.Empty(board.Ships);
    }

    [Fact]
    public void PlaceShip_DuplicateShipType_Fails()
    {
        // Arrange
        var board = new Board();
        var shipA = new Ship(ShipType.Submarine, new Coordinate(1, 1), Orientation.Vertical);
        var shipB = new Ship(ShipType.Submarine, new Coordinate(5, 5), Orientation.Horizontal);

        // Act
        board.PlaceShip(shipA);
        var result = board.PlaceShip(shipB);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PlacementErrors.DuplicateShipType, result.Error);
        Assert.Single(board.Ships);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(10, 0, false)]
    [InlineData(0, 10, false)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    public void IsInBounds_ChecksBothAxes(int column, int row, bool expected)
    {
        // Arrange
        var board = new Board();

        // Act
        var result = board.IsInBounds(new Coordinate(column, row));

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PlaceShip_ValidShip_SucceedsAndIsStoredOnBoard()
    {
        // Arrange
        var board = new Board();
        var ship = new Ship(ShipType.Destroyer, new Coordinate(3, 4), Orientation.Horizontal);

        // Act
        var result = board.PlaceShip(ship);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ship, result.Value);
        Assert.Equal([ship], board.Ships);
    }

    [Fact]
    public void PlaceShip_ShipsCrossingInTheMiddle_FailsWithOverlapAndLeavesBoardUnchanged()
    {
        // Arrange
        var board = new Board();
        var carrier = new Ship(ShipType.Carrier, new Coordinate(0, 2), Orientation.Horizontal);
        var battleship = new Ship(ShipType.Battleship, new Coordinate(2, 0), Orientation.Vertical);
        Assert.True(board.PlaceShip(carrier).IsSuccess);

        // Act
        var result = board.PlaceShip(battleship);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PlacementErrors.Overlap, result.Error);
        Assert.Equal([carrier], board.Ships);
    }

    [Fact]
    public void PlaceShip_AdjacentShipsThatDoNotShareACell_BothSucceed()
    {
        // Arrange
        var board = new Board();
        var carrier = new Ship(ShipType.Carrier, new Coordinate(0, 0), Orientation.Horizontal);
        var destroyer = new Ship(ShipType.Destroyer, new Coordinate(0, 1), Orientation.Horizontal);

        // Act
        var first = board.PlaceShip(carrier);
        var second = board.PlaceShip(destroyer);

        // Assert
        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(2, board.Ships.Count);
    }

    [Fact]
    public void PlaceShip_DifferentTypesWithTheSameLength_BothSucceed()
    {
        // Arrange
        var board = new Board();
        var cruiser = new Ship(ShipType.Cruiser, new Coordinate(0, 0), Orientation.Horizontal);
        var submarine = new Ship(ShipType.Submarine, new Coordinate(0, 2), Orientation.Horizontal);

        // Act
        var first = board.PlaceShip(cruiser);
        var second = board.PlaceShip(submarine);

        // Assert
        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(2, board.Ships.Count);
    }

    [Fact]
    public void ReceiveShot_AlreadyShotThisSpace_FailsWithAlreadyShotSpace()
    {
        // Arrange
        var board = new Board();
        var coordinate = new Coordinate(1, 1);

        // Act
        board.ReceiveShot(coordinate);
        var result = board.ReceiveShot(coordinate);

        // Assert
        Assert.Equal(ShotErrors.AlreadyShotSpace, result.Error);
    }

    [Fact]
    public void ReceiveShot_ShootingEmptySpot_Misses()
    {
        // Arrange
        var board = new Board();
        var coordinate = new Coordinate(2, 4);

        // Act
        var result = board.ReceiveShot(coordinate);

        // Assert
        Assert.Equal(ShotOutcome.Miss, result.Value.Outcome);
    }

    [Fact]
    public void ReceiveShot_ShootingBattleship_Hits()
    {
        // Arrange
        var board = new Board();
        var coordinate = new Coordinate(2, 2);
        var ship = new Ship(ShipType.Battleship, coordinate, Orientation.Horizontal);

        // Act
        board.PlaceShip(ship);
        var result = board.ReceiveShot(coordinate);

        // Assert
        Assert.Equal(ShotOutcome.Hit, result.Value.Outcome);
    }

    [Fact]
    public void ReceiveShot_ShootAllShipCoordinates_ReturnsSunkenShip()
    {
        // Arrange
        var board = new Board();
        var ship = new Ship(ShipType.Destroyer, new Coordinate(3, 3), Orientation.Horizontal);

        // Act
        board.PlaceShip(ship);
        board.ReceiveShot(new Coordinate(3, 3));
        var result = board.ReceiveShot(new Coordinate(4, 3));

        // Assert
        Assert.Equal(ShotOutcome.Sunk, result.Value.Outcome);
        Assert.Equal(ShipType.Destroyer, result.Value.SunkShipType);
    }

    [Fact]
    public void ReceiveShot_ShootingOutOfBounds_FailsWithOutOfBounds()
    {
        // Arrange
        var board = new Board();
        var coordinate = new Coordinate(Board.Size, Board.Size + 1);

        // Act
        var result = board.ReceiveShot(coordinate);

        // Assert
        Assert.Equal(ShotErrors.OutOfBounds, result.Error);
    }

    [Fact]
    public void AreAllShipsPlaced_ReturnsTrue()
    {
        var board = new Board();

        board.PlaceShip(new Ship(ShipType.Battleship, new Coordinate(1, 1), Orientation.Horizontal));
        Assert.False(board.AreAllShipsPlaced);

        board.PlaceShip(new Ship(ShipType.Carrier, new Coordinate(2, 2), Orientation.Horizontal));
        board.PlaceShip(new Ship(ShipType.Cruiser, new Coordinate(3, 3), Orientation.Horizontal));
        board.PlaceShip(new Ship(ShipType.Destroyer, new Coordinate(1, 2), Orientation.Vertical));
        Assert.False(board.AreAllShipsPlaced);

        board.PlaceShip(new Ship(ShipType.Submarine, new Coordinate(2, 3), Orientation.Vertical));
        Assert.True(board.AreAllShipsPlaced);
    }

    [Fact]
    public void AreAllShipsSunk_EmptyBoard_ReturnsFalse()
    {
        // Arrange
        var board = new Board();

        // Assert
        Assert.False(board.AreAllShipsSunk);
    }

    [Fact]
    public void AreAllShipsSunk_ShipNotFullyHit_ReturnsFalse()
    {
        // Arrange
        var board = new Board();
        var ship = new Ship(ShipType.Destroyer, new Coordinate(0, 0), Orientation.Horizontal);
        Assert.True(board.PlaceShip(ship).IsSuccess);

        // Act
        board.ReceiveShot(new Coordinate(0, 0));

        // Assert
        Assert.False(board.AreAllShipsSunk);
    }

    [Fact]
    public void AreAllShipsSunk_OneOfTwoShipsStillAfloat_ReturnsFalse()
    {
        // Arrange
        var board = new Board();
        var destroyer = new Ship(ShipType.Destroyer, new Coordinate(0, 0), Orientation.Horizontal);
        var submarine = new Ship(ShipType.Submarine, new Coordinate(0, 5), Orientation.Horizontal);
        Assert.True(board.PlaceShip(destroyer).IsSuccess);
        Assert.True(board.PlaceShip(submarine).IsSuccess);

        // Act
        board.ReceiveShot(new Coordinate(0, 0));
        board.ReceiveShot(new Coordinate(1, 0));

        // Assert
        Assert.False(board.AreAllShipsSunk);
    }

    [Fact]
    public void AreAllShipsSunk_EveryPlacedShipFullyHit_ReturnsTrue()
    {
        // Arrange
        var board = new Board();
        var destroyer = new Ship(ShipType.Destroyer, new Coordinate(0, 0), Orientation.Horizontal);
        var submarine = new Ship(ShipType.Submarine, new Coordinate(0, 5), Orientation.Horizontal);
        Assert.True(board.PlaceShip(destroyer).IsSuccess);
        Assert.True(board.PlaceShip(submarine).IsSuccess);

        // Act
        board.ReceiveShot(new Coordinate(0, 0));
        board.ReceiveShot(new Coordinate(1, 0));
        board.ReceiveShot(new Coordinate(0, 5));
        board.ReceiveShot(new Coordinate(1, 5));
        board.ReceiveShot(new Coordinate(2, 5));

        // Assert
        Assert.True(board.AreAllShipsSunk);
    }

    [Fact]
    public void TryPlaceShipsRandomly_PlacesFullFleet()
    {
        // Arrange
        var board = new Board();

        // Act
        var result = board.TryPlaceShipsRandomly(new Random(42));

        // Assert
        Assert.True(result);
        Assert.True(board.AreAllShipsPlaced);
        Assert.Equal(5, board.Ships.Count);
    }

    [Fact]
    public void TryPlaceShipsRandomly_SameSeed_ProducesSameLayout()
    {
        // Arrange
        var boardA = new Board();
        var boardB = new Board();

        // Act
        boardA.TryPlaceShipsRandomly(new Random(42));
        boardB.TryPlaceShipsRandomly(new Random(42));

        // Assert
        Assert.Equal(boardA.Ships, boardB.Ships);
    }

    [Fact]
    public void TryPlaceShipsRandomly_DifferentSeeds_CanProduceDifferentLayouts()
    {
        // Arrange
        var boardA = new Board();
        var boardB = new Board();

        // Act
        boardA.TryPlaceShipsRandomly(new Random(1));
        boardB.TryPlaceShipsRandomly(new Random(2));

        // Assert
        Assert.NotEqual(boardA.Ships, boardB.Ships);
    }

    [Fact]
    public void ReceiveShot_EveryOutcome_CarriesTheShotCoordinate()
    {
        // Arrange
        var board = new Board();
        var destroyer = new Ship(ShipType.Destroyer, new Coordinate(0, 0), Orientation.Horizontal);
        Assert.True(board.PlaceShip(destroyer).IsSuccess);
        Coordinate[] targets = [new(5, 5), new(0, 0), new(1, 0)];

        // Act
        var shots = targets.Select(target => board.ReceiveShot(target).Value).ToList();

        // Assert
        Assert.Equal([ShotOutcome.Miss, ShotOutcome.Hit, ShotOutcome.Sunk], shots.Select(s => s.Outcome));
        Assert.Equal(targets, shots.Select(s => s.Coordinate));
    }
}