namespace Battleship.Domain.Tests;

public class CoordinateTests
{

    [Theory]
    [InlineData(0, 0, "A1")]
    [InlineData(3, 0, "D1")]
    [InlineData(2, 4, "C5")]
    [InlineData(9, 9, "J10")]
    public void TryParse_ValidInput_ReturnsValue(int expectedColumn, int expectedRow, string input)
    {
        // Act
        var result = Coordinate.TryParse(input, out var coordinate);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedColumn, coordinate.Column);
        Assert.Equal(expectedRow, coordinate.Row);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    [InlineData("A0")]
    [InlineData("€11")]
    [InlineData("1B")]
    [InlineData("A 1")]
    [InlineData("A-1")]
    public void TryParse_InvalidInput_ReturnsFalse(string? input)
    {
        // Act
        var result = Coordinate.TryParse(input, out var coordinate);

        // Assert
        Assert.False(result);
        Assert.Equal(default, coordinate);
    }


    [Theory]
    [InlineData(0, 0, "A1")]
    [InlineData(3, 0, "D1")]
    [InlineData(2, 4, "C5")]
    [InlineData(9, 9, "J10")]
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