namespace Battleship.Domain.Tests;

public class BoardTests()
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
}