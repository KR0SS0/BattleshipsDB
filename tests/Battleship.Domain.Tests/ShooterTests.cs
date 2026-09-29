namespace Battleship.Domain.Tests;

public class ShooterTests
{
    [Fact]
    public void RandomShotAtTargetSide_SameSeed_ShootsSameCell()
    {
        // Arrange
        var random = new Random(10);
        var sameRandom = new Random(10);
        var board = new Board();
        var shooter = new Shooter();

        // Act
        var result = shooter.RandomShotAtTargetSide(board, random);
        var sameResult = shooter.RandomShotAtTargetSide(board, sameRandom);

        // Assert
        Assert.Equal(sameResult, result);
    }

    [Fact]
    public void RandomShotAtTargetSide_ShotCell_IsNotPicked()
    {
        // Arrange
        var board = new Board();
        var shooter = new Shooter();
        var allCells = Board.AllCells;
        var skippedCell = new Coordinate(4, 4);

        // Act: Shoot every, except one
        foreach (var cell in allCells)
        {
            if (cell == skippedCell) continue;
            board.ReceiveShot(cell);
        }

        var shotCell = shooter.RandomShotAtTargetSide(board, new Random());

        // Assert
        Assert.Equal(skippedCell, shotCell);
        Assert.Equal(shotCell, board.UnshotCells()[0]);
    }

    [Fact]
    public void RandomShotAtTargetSide_ShootingWithNoFreeCells_ReturnsNull()
    {
        var board = new Board();
        var shooter = new Shooter();
        var allCells = Board.AllCells;

        // Act: Shoot every, except one
        foreach (var cell in allCells)
        {
            board.ReceiveShot(cell);
        }
        var returnNull = shooter.RandomShotAtTargetSide(board, new());

        // Assert
        Assert.Null(returnNull);
    }
}