namespace Battleship.Domain.Tests;

public class ShipTypeTests
{
    [Theory]
    [InlineData(ShipType.Carrier, 5)]
    [InlineData(ShipType.Battleship, 4)]
    [InlineData(ShipType.Cruiser, 3)]
    [InlineData(ShipType.Submarine, 3)]
    [InlineData(ShipType.Destroyer, 2)]
    public void Length_ReturnsCorrectLength(ShipType shipType, int expectedLength)
    {
        // Act
        var length = shipType.Length();

        // Assert
        Assert.Equal(expectedLength, length);
    }

    [Fact]
    public void Length_InvalidShipType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidShipType = (ShipType)99;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => invalidShipType.Length());
        Assert.Equal("shipType", exception.ParamName);
    }
}