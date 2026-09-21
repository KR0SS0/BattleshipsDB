namespace Battleship.Domain.Tests;

public class CoordinateTests
{
    [Theory]
    [InlineData(0, 1, "A1")]
    [InlineData(3, 1, "D1")]
    [InlineData(2, 4, "C4")]
    [InlineData(9, 10, "J10")]
    public void ToString_FormatsCorrectly(int column, int row, string expected)
    {
        // Arrange
        var coordinate = new Coordinate(column, row);

        // Act
        var result = coordinate.ToString();

        // Assert
        Assert.Equal(expected, result);
    }
}