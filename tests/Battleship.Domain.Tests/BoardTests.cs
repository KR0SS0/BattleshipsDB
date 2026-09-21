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

        // Arrange
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

        // Arrange
        Assert.False(result.IsSuccess);
        Assert.Equal(PlacementErrors.DuplicateShipType, result.Error);
        Assert.Single(board.Ships);
    }
}