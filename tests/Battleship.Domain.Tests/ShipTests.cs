namespace Battleship.Domain.Tests;

public class ShipTests
{
    [Fact]
    public void GetCells_HorizontalBattleship_ExtendsAcrossColumns()
    {
        // Arrange: column 2 (C), row index 3 (row 4 for the player)
        var ship = new Ship(ShipType.Battleship, new Coordinate(2, 3), Orientation.Horizontal);

        // Act
        var cells = ship.GetCells();

        // Assert
        Coordinate[] expected = [new(2, 3), new(3, 3), new(4, 3), new(5, 3)];
        Assert.Equal(expected, cells);
    }

    [Fact]
    public void GetCells_VerticalBattleship_ExtendsDownRows()
    {
        var ship = new Ship(ShipType.Battleship, new Coordinate(2, 3), Orientation.Vertical);

        var cells = ship.GetCells();

        Coordinate[] expected = [new(2, 3), new(2, 4), new(2, 5), new(2, 6)];
        Assert.Equal(expected, cells);
    }

    [Theory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void GetCells_ReturnsOneCellPerShipLength(Orientation orientation)
    {
        foreach (var type in Enum.GetValues<ShipType>())
        {
            var ship = new Ship(type, new Coordinate(1, 1), orientation);

            var cells = ship.GetCells();

            Assert.Equal(type.Length(), cells.Count);
        }
    }

    [Fact]
    public void Ships_WithSameValues_AreEqual()
    {
        var a = new Ship(ShipType.Cruiser, new Coordinate(2, 2), Orientation.Vertical);
        var b = new Ship(ShipType.Cruiser, new Coordinate(2, 2), Orientation.Vertical);

        Assert.Equal(a, b);
    }
}
