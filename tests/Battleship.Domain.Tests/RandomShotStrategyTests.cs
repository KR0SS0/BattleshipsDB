namespace Battleship.Domain.Tests;

public class RandomShotStrategyTests
{
    [Fact]
    public void ChooseTarget_SameSeed_ShootsSameCell()
    {
        // Arrange
        var random = new Random(10);
        var sameRandom = new Random(10);
        List<Shot> shotHistory = [];
        var shooter = new RandomShotStrategy();

        // Act
        var result = shooter.ChooseTarget(shotHistory, random);
        var sameResult = shooter.ChooseTarget(shotHistory, sameRandom);

        // Assert
        Assert.Equal(sameResult, result);
    }

    [Fact]
    public void ChooseTarget_ShotCell_IsNotPicked()
    {
        // Arrange: Shoot every cell, except one
        var shooter = new RandomShotStrategy();
        var skippedCell = new Coordinate(4, 4);
        var shotHistory = Board.AllCells
            .Where(cell => cell != skippedCell)
            .Select(Shot.Miss)
            .ToList();

        // Act
        var shotCell = shooter.ChooseTarget(shotHistory, new Random());

        // Assert
        Assert.Equal(skippedCell, shotCell);
    }

    [Fact]
    public void ChooseTarget_ShootingWithNoFreeCells_ReturnsNull()
    {
        // Arrange: Shoot every cell
        var shooter = new RandomShotStrategy();
        var shotHistory = Board.AllCells.Select(Shot.Miss).ToList();

        // Act
        var returnNull = shooter.ChooseTarget(shotHistory, new());

        // Assert
        Assert.Null(returnNull);
    }
}
